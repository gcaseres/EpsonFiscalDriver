using System;
namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages
{
	public class EstadoFiscal
	{
		public Boolean ErrorComprobacionMemoriaFiscal;
		public Boolean ErrorComprobacionMemoriaTrabajo;
		public Boolean PocaBateria;
		public Boolean ComandoNoReconocido;
		public Boolean CampoDeDatosInvalido;
		public Boolean ComandoNoValidoParaEstadoFiscal;
		public Boolean DesbordamientoDeTotales;
		public Boolean MemoriaFiscalLlena;
		public Boolean MemoriaFiscalCasiLlena;
		public Boolean ImpresorFiscalCertificado;
		public Boolean ImpresorFiscalFiscalizado;

		public Boolean ModoEntrenamiento
		{
			get {
				return this.ImpresorFiscalCertificado && !this.ImpresorFiscalFiscalizado;
			}
		}

		public Boolean DesfiscalizadoPorSoftware
		{
			get {
				return this.ImpresorFiscalFiscalizado && !this.ImpresorFiscalCertificado;
			}
		}

		public Boolean NecesitaCierreTicketOJornadaFiscal;
		public Boolean DocumentoFiscalAbierto;
		public Boolean DocumentoAbiertoEnRolloPapel;

		public Boolean DocumentoFiscalAbiertoEnRolloPapel
		{
			get {
				return this.DocumentoFiscalAbierto && this.DocumentoAbiertoEnRolloPapel;
			}
		}

		public Boolean DocumentoNoFiscalAbiertoEnRolloPapel
		{
			get {
				return !this.DocumentoFiscalAbierto && this.DocumentoAbiertoEnRolloPapel;
			}
		}

		public Boolean ImpresionEnHojaSueltaInicializada;
		public Boolean Error;

		public EstadoFiscal()
		{
		}
	}
}

