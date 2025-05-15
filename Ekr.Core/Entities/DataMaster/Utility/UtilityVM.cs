using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using System.Collections.Generic;

namespace Ekr.Core.Entities.DataMaster.Utility
{
    public class UtilityVM
    {
        public string q { get; set; }
        public string page { get; set; } = "1";
        public int rowPerPage { get; set; } = 10;
    }
    public class Utility2VM : DropdownFilterVM
    {
        public string Parameter { get; set; }
    }
    public class ListDataDropdownServerSide
    {
        public List<DataDropdownServerSide> items { get; set; }
        public int total_count { get; set; }
    }

    public class DataDropdownServerSide
    {
        public int Number { get; set; }
        public int id { get; set; }
        public string text { get; set; }
        public string format_selected { get; set; }
        public string nama_text { get; set; }
    }
    public class Jumlah_Inbox
    {
        public int? Jumlah { get; set; }
    }

    public class DataMaps_ViewModels
    {
        public int? id { get; set; }
        public string label { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string file { get; set; }
        public string nik { get; set; }
        public string alamatlengkap { get; set; }
        public string uid { get; set; }
        public string sn_alat { get; set; }
        public string status { get; set; }
    }

    public class ConvertLatLong_ViewModels
    {
        public List<ResultConvertLatLong_ViewModels> results { get; set; }
        public string status { get; set; }

    }

    public class ResultConvertLatLong_ViewModels
    {
        public List<AddressComponentConvertLatLong_ViewModels> address_components { get; set; }
        public string formatted_address { get; set; }
        public GeomatryConvertLatLong_ViewModels geometry { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }
    public class GeomatryConvertLatLong_ViewModels
    {
        public LocationConvertLatLong_ViewModels location { get; set; }
        public string location_type { get; set; }
        public viewportConvertLatLong_ViewModels viewport { get; set; }
    }
    public class AddressComponentConvertLatLong_ViewModels
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }

    }
    public class LocationConvertLatLong_ViewModels
    {
        public string lat { get; set; }
        public string lng { get; set; }

    }
    public class viewportConvertLatLong_ViewModels
    {
        public LocationConvertLatLong_ViewModels northeast { get; set; }
        public LocationConvertLatLong_ViewModels southwest { get; set; }

    }
    public class CekUsia_ViewModels
    {
        public string Usia { get; set; }
        public string SegementasiUsia { get; set; }
        public string GenerasiUsia { get; set; }

    }

    public class GetDataUnitByTypeAndUnitIdViewModel
    {
        public string Parameter { get; set; }
        public string TypeId { get; set; }
        public string UnitId { get; set; }
        public string RoleId { get; set; }
        public int Page { get; set; } = 1;
        public int Rows { get; set; } = 10;
    }
}
