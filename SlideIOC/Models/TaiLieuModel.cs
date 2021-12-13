using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlideIOC.Models
{
    public class ThuMucModel
    {
        public int ThuMucID { get; set; }
        public string TenThuMuc { get; set; }
        public int ThuMucChaID { get; set; }
        public string TenThuMucCha { get; set; }
        public int STT { get; set; }
        private List<ThuMucModel> _listchildThuMuc = new List<ThuMucModel>();
        public List<ThuMucModel> ListChildThuMuc
        {
            get { return this._listchildThuMuc; }
            set { this._listchildThuMuc = value; }
        }

        private List<TaiLieu> _listTaiLieu = new List<TaiLieu>();
        public List<TaiLieu> ListTaiLieu
        {
            get
            {
                return this._listTaiLieu;
            }
            set
            {
                this._listTaiLieu = value;
            }
        }


    }

    public class UnionThuMucModel
    {
        public int ThuMucID { get; set; }
        public string TenThuMuc { get; set; }
        public int ThuMucChaID { get; set; }
        public string TenThuMucCha { get; set; }
        public int ThuMucLevel { get; set; }
        public int STT { get; set; }
        private List<UnionThuMucModel> _childThuMuc = new List<UnionThuMucModel>();
        public  List<UnionThuMucModel> ChildThuMuc
        {
            get { return this._childThuMuc; }
            set { this._childThuMuc = value; }
        }
    }

    public class TaiLieu
    {
        public int TaiLieuID { get; set; }
        public string TenTaiLieu { get; set; }
        public int ThuMucID { get; set; }
    }

    public class DetailTaiLieuModel
    {
        public int TaiLieuID { get; set; }
        public string TenTaiLieu { get; set; }
        public string NoiDungTaiLieu { get; set; }
        public int ThuMucID { get; set; }
        public int STT { get; set; }
        public HttpPostedFileWrapper HtmlNoiDung { get; set; }

        private List<Files> _files = new List<Files>();
        public List<Files> Files
        {
            get
            {
                return this._files;
            }
            set
            {
                this._files = value;
            }
        }
    }


    public class Files
    {
        public int TINNHAN_ID { get; set; }
        public string FILENAME { get; set; }
        public string FILETYPE { get; set; }
    }

}