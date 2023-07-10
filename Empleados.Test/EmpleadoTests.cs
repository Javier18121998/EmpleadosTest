using Castle.Core.Logging;
using Empleados.API.Controllers;
using Empleados.Core.Modelos;
using Empleados.Infraestructura.Data;
using Empleados.Infraestructura.Repositorio;
using Empleados.Infraestructura.Repositorio.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empleados.Test
{
    [TestFixture]
    public class EmpleadoTests
    {
        private Empleado empleadoTest1;
        private Empleado empleadoTest2;
        private DbContextOptions<ApplicationDbContext> options;

        [SetUp]
        public void Setup()
        {
            options = new DbContextOptionsBuilder<ApplicationDbContext>()
                          .UseInMemoryDatabase(databaseName: "temp_empleadoDB").Options;
            empleadoTest1 = new Empleado()
            {
                Id = 1,
                Apellidos = "Piedra 1",
                Nombres = "Carlos 1",
                Cargo = "Desarrollador",
                CompaniaId = 1,
            };
            empleadoTest2 = new Empleado()
            {
                Id = 2,
                Apellidos = "Piedra 2",
                Nombres = "Carlos 2",
                Cargo = "Desarrollador",
                CompaniaId = 1,
            };
        }

        [Test]
        [Order(1)]
        public async Task EmpleadoRepositorio_AgregarEmpleado_GrabadoExitoso()
        {
            //Arrange
            var context = new ApplicationDbContext(options);
            var empleadoRepositorio = new EmpleadoRepositorio(context);
            
            //Act
            await empleadoRepositorio.Agregar(empleadoTest1);
            await empleadoRepositorio.Guardar();
            var empleadoDB = await empleadoRepositorio.ObtenerPrimero();
            
            //Assert
            Assert.AreEqual(empleadoTest1.Id, empleadoDB.Id);
            Assert.AreEqual(empleadoTest1.Apellidos, empleadoDB.Apellidos);
        }

        [Test]
        [Order(2)]
        public async Task EmpleadoRepositorio_ObtenerTodos_OntenrListaEmpleados()
        {
            //Arrange
            var expectedResult = new List<Empleado> { empleadoTest1, empleadoTest2 };
            var context = new ApplicationDbContext(options);
            var empleadoRepositorio = new EmpleadoRepositorio(context);
            context.Database.EnsureDeleted();

            //Act
            await empleadoRepositorio.Agregar(empleadoTest1);
            await empleadoRepositorio.Agregar(empleadoTest2);
            await empleadoRepositorio.Guardar();
            var empleadoLista = await empleadoRepositorio.ObtenerTodos();

            //Assert
            CollectionAssert.AreEqual(expectedResult, empleadoLista);
        }
        [Test]
        [Order(3)]
        public async Task EmpleadoController_GetEmpleados_ObtenerListaEmpleados()
        {
            //Arrange
            var empleados = new List<Empleado>()
            {
                new Empleado() 
                { 
                    Id = 1,
                    Apellidos = "Apellidos Test1",
                    Nombres = "Nombres Test1",
                    Cargo = "Cargo 1",
                    CompaniaId = 1
                },
                new Empleado()
                {
                    Id = 2,
                    Apellidos = "Apellidos Test2",
                    Nombres = "Nombres Test2",
                    Cargo = "Cargo 2",
                    CompaniaId = 1
                }
            };

            //Act
            var mockEmpleadoRepositorio = new Mock<IEmpleadoRepositorio>();
            mockEmpleadoRepositorio.Setup(x => x.ObtenerTodos(null, null, "Compania")).ReturnsAsync(empleados);
            var mockLogger = new Mock<ILogger<EmpleadoController>>();
            var empleadoController = new EmpleadoController(mockEmpleadoRepositorio.Object, mockLogger.Object);
            var actionResult = await empleadoController.GetEmpleados();
            var resultado = actionResult.Result as OkObjectResult;
            var empleadosDB = resultado.Value as IEnumerable<Empleado>;

            //
            CollectionAssert.AreEqual(empleados, empleadosDB);
            Assert.AreEqual(empleados.Count(), empleadosDB.Count());
        }
    }
}
