using System;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Drivers;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Tickets;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.ControlFiscal;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Messages;

namespace Sandbox
{
	class MainClass
	{
		public static void Main(string[] args)
		{		

			Driver cn = new Driver("COM7");
			cn.MessageReceived += Connection_MessageReceived;

			try {
				cn.Open();
			} catch {
				Console.WriteLine("ERROR: No se pudo abrir la conexion");
				System.Console.ReadKey();
				return;			
			}
			/*
			cn.Send(new AbrirOperation(false).GetBytes());

			ImprimirItemOperation op = new ImprimirItemOperation(
				"Item de prueba",
				1,
				121.0m,
				2100, 
				CalificadorLineaItem.MontoAgregadoMercaderia
			);
			cn.Send(op.GetBytes());

			cn.Send(new CerrarOperation(true).GetBytes());

			Console.ReadKey();
*/

			for (int i =0; i<=0; i++) {
				try {
					Console.WriteLine("Iteracion {0}", i);

					byte[] response;
					Message m;

					Console.WriteLine("Enviando comando CANCELAR COMPROBANTE");

					response = cn.BlockSend(new PagoOperation(
						"",
						0.0m,
						CalificadorPago.CancelarComprobante
					).GetBytes());
					m = GeneralMessageFactory.Instance.CreateFromBytes(response);
					Console.WriteLine(m);

					Console.WriteLine("Enviando comando CIERRE Z");
				response = cn.BlockSend(new CierreJornadaOperation(TipoCierreJornada.CierreZ, true).GetBytes());
				m = GeneralMessageFactory.Instance.CreateFromBytes(response);
				Console.WriteLine(m);
				
					/*

					Console.WriteLine("Enviando comando ABRIR TICKET");
					response = cn.BlockSend(new AbrirOperation(false).GetBytes());
					m = GeneralMessageFactory.Instance.CreateFromBytes(response);
					Console.WriteLine(m);


					Console.WriteLine("Enviando comando IMPRIMIR ITEM");

					ImprimirItemOperation op = new ImprimirItemOperation(
				"Item de prueba",
				1,
				1.0m,
				2100, 
				CalificadorLineaItem.MontoAgregadoMercaderia
					);


					response = cn.BlockSend(op.GetBytes());
					m = GeneralMessageFactory.Instance.CreateFromBytes(response);
					Console.WriteLine(m);


					Console.WriteLine("Enviando comando PAGO");

					response = cn.BlockSend(new PagoOperation(
						"PESOS",
						1.0m,
						CalificadorPago.SumaImportePagado
					).GetBytes());
					m = GeneralMessageFactory.Instance.CreateFromBytes(response);
					Console.WriteLine(m);

					Console.WriteLine("Enviando comando CERRAR TICKET");
					response = cn.BlockSend(new CerrarOperation(true).GetBytes());
					m = GeneralMessageFactory.Instance.CreateFromBytes(response);
					Console.WriteLine(m);
*/
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
			/*
			cn.Send(new AbrirOperation(false).GetBytes());
			
			Console.WriteLine("Presione una tecla");
			System.Console.ReadKey();
			
			ImprimirItemOperation op = new ImprimirItemOperation(
				"Item de prueba",
				1,
				121.0m,
				2100, 
				CalificadorLineaItem.MontoAgregadoMercaderia
				);
			
			cn.Send(
				op.GetBytes()
			);
			
			Console.WriteLine("Presione una tecla");
			System.Console.ReadKey();
			
			cn.Send(new CerrarOperation(true).GetBytes());
			
			Console.WriteLine("Presione una tecla");
			System.Console.ReadKey();

			cn.Send(new CierreJornadaOperation(TipoCierreJornada.CierreZ, true).GetBytes());
			
			Console.WriteLine("Presione una tecla");
			System.Console.ReadKey();

*/
			Console.WriteLine("Cerrando...");
			cn.Close();

			
		}
		
		protected static void Connection_MessageReceived(object sender, MessageReceviedEventArgs e)
		{
			Message m = GeneralMessageFactory.Instance.CreateFromBytes(e.MessageData);
			Console.WriteLine(m.ToString());
		}		
	}
}
