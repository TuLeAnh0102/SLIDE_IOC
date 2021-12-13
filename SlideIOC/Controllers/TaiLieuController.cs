
using SlideIOC.DataAccess;
using SlideIOC.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace SlideIOC.Controllers
{
    public class TaiLieuController : Controller
    {
        TaiLieuAccess accTaiLieu = new TaiLieuAccess();

        #region User
        
        public ActionResult Index()
        {
            List<ThuMucModel> lstThuMuc = new List<ThuMucModel>();
            ResultEvent resultThuMuc = new ResultEvent();
            resultThuMuc = accTaiLieu.GetAllThuMuc();
            if(resultThuMuc.Error == 0)
            {
                lstThuMuc = resultThuMuc.DataResult.DataTableToList<ThuMucModel>();
                ResultEvent resultTaiLieu = new ResultEvent();
                foreach (ThuMucModel item in lstThuMuc)
                {
                    resultTaiLieu = accTaiLieu.GetTaiLieuByThuMuc(item.ThuMucID);
                    if (resultTaiLieu.Error == 0)
                    {
                        item.ListTaiLieu = resultTaiLieu.DataResult.DataTableToList<TaiLieu>();
                    }
                }

            }

            List<ThuMucModel> lstThuMucSort = new List<ThuMucModel>();
            SortThuMuc1(lstThuMuc, 0, ref lstThuMucSort);
            ViewData["DanhSachThuMuc"] = lstThuMucSort;

            return View(lstThuMuc);
        }

        public FileResult DisplayPDF()
        {
            string filepath = Server.MapPath("/Image/AAA.PDF");
            byte[] pdfByte = GetBytesFromFile(filepath);
            return File(pdfByte, "application/pdf");
        }

        private byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)
            FileStream fs = null;
            try
            {
                fs = System.IO.File.OpenRead(fullFilePath);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        [HttpGet]
        public ActionResult DetailTaiLieu(int id)
        {
            DetailTaiLieuModel resultModel = new DetailTaiLieuModel();
            ResultEvent resultTaiLieu = new ResultEvent();
            resultTaiLieu = accTaiLieu.GetDetailTaiLieuById(id);
            if (resultTaiLieu.Error == 0 && resultTaiLieu.DataResult.Rows.Count > 0)
            {
                resultModel = resultTaiLieu.DataResult.DataTableToList<DetailTaiLieuModel>()[0];
                //get all tài liệu
                ResultEvent resultFile = new ResultEvent();
                resultFile = accTaiLieu.GetFileByTaiLieu(resultModel.TaiLieuID);
                if (resultFile.Error == 0 && resultFile.DataResult.Rows.Count > 0)
                {
                    resultModel.Files = resultFile.DataResult.DataTableToList<Files>();
                }
            }

            return PartialView("_DetailTaiLieu", resultModel);
        }
        #endregion

        #region Admin
        public ActionResult Admin()
        {
            LoginModel model = new LoginModel();

            if (this.Session["UserProfile"] != null)
            {
                RedirectToAction("Index", "PhieuKhaoSat");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Admin(LoginModel model)
        {

            if (Session["Captcha"] == null || Session["Captcha"].ToString() != model.Captcha)
            {
                model.Captcha = "";
                ViewBag.Message = "Mã bảo vệ không đúng. Vui lòng nhập lại";
            }
            else
            {
                WebServiceAccountVNPT.AccountService webService = new WebServiceAccountVNPT.AccountService();
                WebServiceAccountVNPT.Account userLogin = new WebServiceAccountVNPT.Account();

                //userLogin = webService.Login(model.UserName, model.Password);

                userLogin.Result = 1;

                if (userLogin.Result == 1)
                {
                    UserProfile user = new UserProfile();
                    user.HoTen = "Võ Văn Sang";
                    user.TaiKhoan = "sangvv.bpc";
                    user.Ten_DV = "CNTT";
                    user.Id_DV = 1;
                    //user.HoTen = userLogin.Name;
                    //user.TaiKhoan = userLogin.UserName;
                    //user.Ten_DV = userLogin.Ten_dv;
                    //user.Id_DV = userLogin.DonviID;
                    user.Image = "";


                    ResultEvent resultFile = new ResultEvent();
                    resultFile = accTaiLieu.CheckTaiKhoanAdmin("sangvv.bpc");
                    if (resultFile.Error == 0 && resultFile.DataResult.Rows.Count > 0)
                    {
                        this.Session["UserProfile"] = user;
                        return RedirectToAction("QuanLiTaiLieu", "TaiLieu");
                    }
                }
                else
                {
                    ViewBag.Message = "Tài khoản và mật khẩu không hợp lệ. Vui lòng kiểm tra lại";
                }
            }

            return View(model);

        }

        public ActionResult Logout()
        {
            this.Session.Clear();

            return RedirectToAction("Admin", "TaiLieu");
        }

        public ActionResult QuanLiTaiLieu()
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            List<ThuMucModel> lstThuMuc = new List<ThuMucModel>();
            ResultEvent resultThuMuc = new ResultEvent();
            resultThuMuc = accTaiLieu.GetAllThuMuc();


            if (resultThuMuc.Error == 0)
            {
                lstThuMuc = resultThuMuc.DataResult.DataTableToList<ThuMucModel>();
                ResultEvent resultTaiLieu = new ResultEvent();
                foreach (ThuMucModel item in lstThuMuc)
                {
                    resultTaiLieu = accTaiLieu.GetTaiLieuByThuMuc(item.ThuMucID);
                    if (resultTaiLieu.Error == 0)
                    {
                        item.ListTaiLieu = resultTaiLieu.DataResult.DataTableToList<TaiLieu>();
                    }
                }

            }
            ViewData["DanhSachThuMuc"] = lstThuMuc;
            return View(lstThuMuc);
        }

        [HttpGet]
        public ActionResult EditDetailThuMuc(int id, int pa)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            ThuMucModel resultModel = new ThuMucModel();
            if (id != 0)
            {
                ResultEvent resultTaiLieu = new ResultEvent();
                resultTaiLieu = accTaiLieu.GetDetailThuMucById(id);
                if (resultTaiLieu.Error == 0 && resultTaiLieu.DataResult.Rows.Count > 0)
                {
                    resultModel = resultTaiLieu.DataResult.DataTableToList<ThuMucModel>()[0];
                }
            }
            else
            {
                if (pa != 0)
                    resultModel.ThuMucChaID = pa;
            }

            //get value combobox
            List<UnionThuMucModel> lstThuMuc = new List<UnionThuMucModel>();
            var selectListThuMuc = new List<SelectListItem>();

            ResultEvent resultThuMuc = new ResultEvent();
            resultThuMuc = accTaiLieu.GetAllThuMuc();
            if (resultThuMuc.Error == 0)
            {
                lstThuMuc = resultThuMuc.DataResult.DataTableToList<UnionThuMucModel>();
                selectListThuMuc = lstThuMuc.Select(x => new SelectListItem
                {
                    Value = x.ThuMucID.ToString(),
                    Text = x.TenThuMuc
                }).ToList();
            }

            ViewData["CmbListThuMuc"] = new SelectList(selectListThuMuc, "Value", "Text", 0);

            return PartialView("_EditDetailThuMuc", resultModel);
        }

        [HttpPost]
        public ActionResult EditDetailThuMuc(ThuMucModel model)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            int result = -1;
            if (model.ThuMucID == 0)
                result = accTaiLieu.InsertThuMuc(model);
            else
                result = accTaiLieu.UpdateThuMuc(model);

            if (result == 1)
                return Json(new { success = true, responseText = "Đã lưu thành công" }, JsonRequestBehavior.AllowGet);

            return Json(new { success = false, responseText = "Đã có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveThuMuc(String data)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            int result = -1;


            if (result == 1)
            {

                return Json(new { success = true, responseText = "Lưu thành công" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, responseText = "Lưu thất bại" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpDelete]
        public ActionResult DeleteThuMuc(int id)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            ResultEvent resultTaiLieu = new ResultEvent();

            resultTaiLieu = accTaiLieu.GetTaiLieuByThuMuc(id);
            if (resultTaiLieu.Error == 0)
            {
                if (resultTaiLieu.DataResult.Rows.Count > 0)
                {
                    return Json(new { success = false, responseText = "Thư mục có chứa tài liệu nên không thể xóa" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (accTaiLieu.DeleteThuMuc(id) == -1)
                    {
                        return Json(new { success = false, responseText = "Đã có lỗi xảy ra khi xóa Thư mục" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = true, responseText = "Lưu thành công" }, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            return RedirectToAction("QuanLiTaiLieu");
        }

        [HttpGet]
        public ActionResult EditDetailTaiLieu(int id, int pa)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            DetailTaiLieuModel resultModel = new DetailTaiLieuModel();
            if (id != 0)
            {
                ResultEvent resultTaiLieu = new ResultEvent();
                resultTaiLieu = accTaiLieu.GetDetailTaiLieuById(id);
                if (resultTaiLieu.Error == 0 && resultTaiLieu.DataResult.Rows.Count > 0)
                {
                    resultModel = resultTaiLieu.DataResult.DataTableToList<DetailTaiLieuModel>()[0];
                }
            }
            else
            {
                if (pa != 0)
                    resultModel.ThuMucID = pa;
            }

            //get value combobox
            List<UnionThuMucModel> lstThuMuc = new List<UnionThuMucModel>();
            var selectListThuMuc = new List<SelectListItem>();

            ResultEvent resultThuMuc = new ResultEvent();
            resultThuMuc = accTaiLieu.GetAllThuMuc();
            if (resultThuMuc.Error == 0)
            {
                SortThuMuc(resultThuMuc.DataResult.DataTableToList<UnionThuMucModel>(), 1, ref lstThuMuc);

                selectListThuMuc = lstThuMuc.Select(x => new SelectListItem
                {
                    Value = x.ThuMucID.ToString(),
                    Text = x.TenThuMuc
                }).ToList();
            }

            ViewData["CmbListThuMuc"] = new SelectList(selectListThuMuc, "Value", "Text", 0);


            return PartialView("_EditDetailTaiLieu", resultModel);
        }

        [HttpPost]
        public ActionResult EditDetailTaiLieu(DetailTaiLieuModel model)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            int result = -1;
            result = accTaiLieu.InsertTaiLieu(model);
            if (result == 1)
            {
                return View();
            }
            return View();
        }

        [HttpPost]
        public ActionResult SaveTaiLieu([System.Web.Http.FromBody] DetailTaiLieuModel tailieuModel, List<HttpPostedFileBase> files)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }
            if (tailieuModel.HtmlNoiDung != null && tailieuModel.HtmlNoiDung.ContentLength > 0 && tailieuModel.HtmlNoiDung.ContentType == "text/xml")
            {
                BinaryReader b = new BinaryReader(tailieuModel.HtmlNoiDung.InputStream);
                byte[] binData = b.ReadBytes(tailieuModel.HtmlNoiDung.ContentLength);

                tailieuModel.NoiDungTaiLieu = System.Text.Encoding.UTF8.GetString(binData);
            }
            int result = -1;
            if (tailieuModel.TaiLieuID == 0)
            {
                result = accTaiLieu.InsertTaiLieu(tailieuModel, files, Server.MapPath("~/Files/"));
            }
            else
            {
                result = accTaiLieu.UpdateTaiLieu(tailieuModel);
            }


            if (result == 1)
            {

                return Json(new { success = true, responseText = "Lưu thành công" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, responseText = "Lưu thất bại" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpDelete]
        public ActionResult DeleteTaiLieu(int id)
        {
            if (this.Session["UserProfile"] == null)
            {
                return RedirectToAction("Index", "TaiLieu");
            }

            if (accTaiLieu.DeleteTaiLieu(id) == -1)
            {
                return Json(new { success = false, responseText = "Đã có lỗi xảy ra khi xóa thư mục này" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, responseText = "Lưu thành công" }, JsonRequestBehavior.AllowGet);
            }
        }

        private void SortThuMuc(List<UnionThuMucModel> lstTree, int rootId, ref List<UnionThuMucModel> resultTree)
        {
            if (lstTree == null || lstTree.Count == 0)
            {
                return;
            }
            UnionThuMucModel iThuMuc = new UnionThuMucModel();
            iThuMuc = lstTree.First(a => a.ThuMucID.Equals(rootId));
            resultTree.Add(iThuMuc);
            var subChild = lstTree.Where(a => a.ThuMucChaID.Equals(iThuMuc.ThuMucID) && !a.ThuMucChaID.Equals(a.ThuMucID)).Count();
            if (subChild > 0)
            {
                foreach (var i in lstTree.Where(a => a.ThuMucChaID.Equals(iThuMuc.ThuMucID) && !a.ThuMucChaID.Equals(a.ThuMucID)))
                {
                    SortThuMuc(lstTree, i.ThuMucID, ref resultTree);
                }
            }
        }

        private void SortThuMuc1(List<ThuMucModel> lstTree, int rootId, ref List<ThuMucModel> resultTree)
        {
            if (lstTree == null || lstTree.Count == 0)
            {
                return;
            }
            resultTree = lstTree.Where(a => a.ThuMucChaID.Equals(rootId) && !a.ThuMucChaID.Equals(a.ThuMucID)).ToList();

            foreach (var item in resultTree)
            {
                List<ThuMucModel> lstChildThuMuc = new List<ThuMucModel>();
                SortThuMuc1(lstTree, item.ThuMucID, ref lstChildThuMuc);
                item.ListChildThuMuc = lstChildThuMuc;
            }
        }

        public ActionResult CaptchaImage(string prefix, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question 
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            //store answer 
            Session["Captcha" + prefix] = a + b;

            //image stream 
            FileContentResult img = null;

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                //add noise 
                if (noisy)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)));

                        r = rand.Next(0, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);

                        gfx.DrawEllipse(pen, x - r, y - r, r, r);
                    }
                }

                //add question 
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg 
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = this.File(mem.GetBuffer(), "image/Jpeg");
            }

            return img;
        }
        #endregion

    }
}