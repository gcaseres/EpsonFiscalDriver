using System;
namespace Snappminds.ImpresorasFiscales.EpsonTM2000.Messages
{
	public class EstadoImpresora
	{
		public Boolean FallaDeImpresora;
		public Boolean ImpresoraFueraDeLinea;
		public Boolean PocoPapelCintaAuditoria;
		public Boolean PocoPapelComprobantes;
		public Boolean BufferImpresoraLleno;
		public Boolean BufferImpresoraVacio;
		public Boolean EntradaHojasSueltasFrontalPreparada;
		public Boolean HojaSueltaFrontalPreparada;
		public Boolean TomaDeHojasParaValidacionPreparada;
		public Boolean PapelParaValidacionPresente;
		public Boolean CajonDeDineroAbierto;
		public Boolean ImpresoraSinPapel;
		public Boolean Error;

		public EstadoImpresora()
		{
		}
	}
}

