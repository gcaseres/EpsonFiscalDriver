using System;
using System.Collections.Generic;
using Snappminds.ImpresorasFiscales.EpsonTM2000.Messages.ControlFiscal;


namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages
{
	/// <summary>
	/// Factory que delega la construccion de mensajes a otras factories
	/// en funcion del codigo de mensaje recibido.
	/// </summary>
	public class GeneralMessageFactory : IMessageFactory
	{
		private static GeneralMessageFactory _Instance;

		public static GeneralMessageFactory Instance
		{
			get {
				if (_Instance == null)
					_Instance = new GeneralMessageFactory();

				return _Instance;
			}
		}

		private Dictionary<byte,IMessageFactory> _FactoryRegistry;

		public GeneralMessageFactory()
		{
			this.FactoryRegistry = new Dictionary<byte, IMessageFactory>();
			this.RegisterFactories();
		}

		protected Dictionary<byte, IMessageFactory> FactoryRegistry
		{
			set {
				this._FactoryRegistry = value;
			}
			get {
				return this._FactoryRegistry;
			}
		}

		protected void RegisterFactories()
		{
			this.FactoryRegistry[(byte)0x39] = new CierreJornadaMessageFactory();
		}

		public Message CreateFromBytes(byte[] data)
		{
			try {
				return this.FactoryRegistry[data[0]].CreateFromBytes(data);
			} catch {
				return (new MessageFactory()).CreateFromBytes(data);
			}
		}
	}
}

