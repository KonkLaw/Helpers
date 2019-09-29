namespace RouteByApi
{
	internal class BuyHelper
	{
		internal static void Buy()
		{
			//if (infoRecord.TicketsCount < 1)
			//	throw new Exception("No tickets");
			//
			//string GetValueByKey(string content, string key)
			//{
			//	const string valueMarker = "value=\\\"";
			//	int index = content.IndexOf(key);
			//	index = content.IndexOf(valueMarker, index) + valueMarker.Length;
			//	int endIndex = content.IndexOf("\\\"", index);
			//	return content.Substring(index, endIndex - index);
			//}
			//
			//// IN - POSADKA
			//// OUT - VISADKA
			//
			//const string stobcy = "102";
			//const string minsk = "1";
			//string inBus;
			//string outBus;
			//if (fromMinskToStol)
			//{
			//	inBus = minsk;
			//	outBus = stobcy;
			//}
			//else
			//{
			//	inBus = stobcy;
			//	outBus = minsk;
			//}
			//
			//
			//string preorderRequestContent = $"type=load_step2&load_in_page=true&id_tt={infoRecord.Id}&num_selected=1&select_in={inBus}&select_out={outBus}&sline=undefined&idtemp=undefined&timer=undefined";
			//WebRequest preorderRequest = routieBySessionHelper.Request(preorderRequestContent);
			//string infoResponse = WebApiHelper.GetResponseString(preorderRequest).DecodeUnicide();
			//
			//string GetValue(string key) => GetValueByKey(infoResponse, key);
			//string part = "%5B" + GetValueByKey(infoResponse, "aurb_id_add_parts")[1] + "%5D";
			//string orderRequestContent =
			//	@"type=load_step2_save&" +
			//	$"aurb_id_add_et={GetValue("aurb_id_add_et")}&" +
			//	"aurb_id_add_num_space=1&" +
			//	$"aurb_id_add_tt={GetValue("aurb_id_add_tt")}&" +
			//	$"aurb_id_add_parts={part}&" +
			//	"aurb_points_finish[1]=9761&" +
			//	"aurb_point_start[1]=9716&" +
			//	$"aurb_id_add_df={GetValue("aurb_id_add_df")}&" +
			//	$"aurb_id_add_ds={GetValue("aurb_id_add_ds")}&" +
			//	"aurb_id_add_comment=&" +
			//	$"aurb_id_add_sl={GetValue("aurb_id_add_sl")}&" +
			//	$"aurb_id_add_save_points={GetValue("aurb_id_add_save_points")}&" +
			//	"aurb_id_service=144";
			//
			//WebRequest orderRequest = routieBySessionHelper.Request(orderRequestContent);
			//string orderResponce = WebApiHelper.GetResponseString(orderRequest).DecodeUnicide();
		}
	}
}
