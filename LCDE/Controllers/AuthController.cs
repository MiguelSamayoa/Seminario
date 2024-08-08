﻿using LCDE.Models;
using LCDE.Models.Enums;
using LCDE.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace LCDE.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IRepositorioUsuarios repositorioUsuarios;
        private readonly IRepositorioCliente repositorioCliente;
        private readonly IRepositorioToken repositorioToken;
        private readonly IEmailService emailService;

        public AuthController(UserManager<Usuario> userManager, IRepositorioUsuarios repositorioUsuarios, IRepositorioCliente repositorioCliente,
            IRepositorioToken repositorioToken, IEmailService emailService,
            SignInManager<Usuario> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.repositorioCliente = repositorioCliente;
            this.repositorioToken = repositorioToken;
            this.emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Registro(CrearUsarioCliente modelo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(modelo);
                }

                var usuario = new Usuario()
                {
                    Correo = modelo.informacionUsuario.Correo,
                    Nombre_usuario = modelo.informacionUsuario.Nombre_usuario,
                    Id_Role = (int)Rol.Cliente
                };
                var resultadoUsuario = await userManager.CreateAsync(usuario, password: modelo.informacionUsuario.Contrasennia);

                var cliente = new Cliente()
                {
                    Correo = modelo.informacionUsuario.Correo,
                    Nombre = modelo.informacionCliente.Nombre,
                    Direccion = modelo.informacionCliente.Direccion,
                    Telefono = modelo.informacionCliente.Telefono,
                    Id_usuario = usuario.Id
                };
                var resultadoCliente = await repositorioCliente.CrearCliente(cliente);

                var token = repositorioToken.CrearTokenRegistroUsuario(usuario);

                try
                {
                    //Aqui debo de implementar la plantilla que no me ha hecho junior :,( ni la plantilla ni nada.
                    //falta sustituir los valores de la plantilla 
                    //generar Urla segun fernando que no sé pa que va a servir, pero gary debe componer el script xD
                    await emailService.SendEmailAsync(usuario.Correo, "Confirmar registro", "La plantilla que falta señores, huevones xD");
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error", "Home");
                }
                if (resultadoUsuario.Succeeded && resultadoCliente != 0)

                {

                    return RedirectToAction("Login");
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Ocurrió un error, intente más tarde.");

                    return View(modelo);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            try
            {
                var modelo = new CrearUsarioCliente()
                {
                    informacionUsuario = new RegistroUsuarioCliente(),
                    informacionCliente = new CrearClienteDTo()
                };
                return View(modelo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Unauthorized()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(modelo);
                }

                var resultado = await signInManager.PasswordSignInAsync(modelo.Email,
                        modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);

                if (resultado.Succeeded)
                {
                    var usuario = await repositorioUsuarios.BuscarUsuarioPorEmail(modelo.Email);
                    var claims = new List<Claim>
                        {
                            new(ClaimTypes.Name, usuario.Nombre_usuario), // Agregar el claim del nombre
                            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()), // Agregar el claim del Id de usuario
                            new(ClaimTypes.Role, usuario.Id_Role.ToString())
                        };

                    var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Crear un nuevo principal con los claims adicionales
                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));


                    //await signInManager.SignInAsync(usuario, modelo.Recuerdame);
                    return RedirectToAction("Index", "Ventas");
                }

                ModelState.AddModelError(string.Empty, "Credenciales no válidas");
                return View(modelo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerRoles()
        {
            return await repositorioUsuarios.ObtenerRoles();
        }
    }
}