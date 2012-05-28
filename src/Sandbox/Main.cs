using System;
using Snappminds.ImpresorasFiscales.Drivers.EpsonTM2000;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Operations.Tickets;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sandbox
{
	class MainClass
	{
		public static void Main(string[] args)
		{		

			Driver cn = new Driver("COM7");
			cn.MessageReceived += Connection_MessageReceived;
			
			cn.Open();
			
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
			
			Console.WriteLine("Cerrando...");
			cn.Close();
			
		}
		
		protected static void Connection_MessageReceived(object sender, MessageReceviedEventArgs e)
		{
			string data = "";
			foreach (byte element in e.Message) {
				data += element.ToString("X") + ",";
			}
			System.Console.WriteLine("Mensaje recibido {0}", data);
		}		
	}
}
