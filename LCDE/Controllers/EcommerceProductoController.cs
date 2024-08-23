﻿using LCDE.Models;
using LCDE.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace LCDE.Controllers
{
    public class EcommerceProductoController : Controller
    {
        private readonly RepositorioProductos repositorioProductos;
        private readonly RepositorioCategorias repositorioCategorias;


        public EcommerceProductoController(

            RepositorioProductos repositorioProductos, RepositorioCategorias repositorioCategorias)
        {
            this.repositorioProductos = repositorioProductos;
            this.repositorioCategorias = repositorioCategorias;
        }
        public async Task<IActionResult> DetalleProducto(int productoId)
        {
            try
            {
                //ProductoListarDTO producto = await repositorioProductos.ObtenerDetalleProducto(productoId);
                var producto = new ProductoListarDTO()
                {
                    Id = 1,
                    Nombre = "Producto 1",
                    Detalle = "Detalles extensos del producto.",
                    Image_url = "https://resourcesdev.blob.core.windows.net/resources-web/proyectos/banner_img_01.jpg",
                    Existencia = 10,
                    Id_Categoria = 1,
                    Id_Proveedor = 1,
                    IdPrecio = 1,
                    IdPromocion = 1,
                    NombreCategoria = "Categoria 1",
                    NombreProveedor = "Proveedor 1",
                    PrecioUnidad = 100,
                    Promocion = "Promocion 1",
                    Descuento = 10
                };

                if (producto == null)
                {
                    return RedirectToAction("NoEncontrado", "Home");
                }
                return View(producto);

            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidarExistenciaSuficiente(int id, int cantidadIngresada)
        {
            Producto respuesta = await repositorioProductos.ObtenerProducto(id);
            if (respuesta is null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Ocurrió un problema al consultar el producto");
            }
            if (cantidadIngresada > respuesta.Existencia)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"La existencia del producto {respuesta.Detalle} no es suficiente, existencia actual: {respuesta.Existencia}");
            }
            return StatusCode(StatusCodes.Status200OK, true);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarPorCategorias(int CategoriaId)
        {
            ProductoFiltroDTO obj = new ProductoFiltroDTO();
            obj.Categorias = (await repositorioCategorias.ObtenerTodosCategorias()).ToList();
            obj.CategoriaId = CategoriaId;
            obj.PrecioMin = 0;
            obj.PrecioMax = await repositorioProductos.PrecioMaximo();
            obj.NombreProducto = "";
            obj.productosListarDTO = await repositorioProductos.ObtenerProductoFiltrado(obj);
            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> BuscarPorCategorias(ProductoFiltroDTO productoFiltrar)
        {
            try
            {
                List<ProductoListarDTO> productos = await repositorioProductos.ObtenerProductoFiltrado(productoFiltrar);
                productoFiltrar.productosListarDTO = productos;

                return StatusCode(200, productoFiltrar);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}