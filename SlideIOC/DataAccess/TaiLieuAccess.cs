using SlideIOC.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SlideIOC.DataAccess
{
    public class TaiLieuAccess : BaseAccess
    {
        #region Tài Liêu
        public int InsertTaiLieu(DetailTaiLieuModel model)
        {
            ResultEvent resultEvent = new ResultEvent();
            String strCommand = "INSERT INTO tailieu (TenTaiLieu, NoiDungTaiLieu, ThuMucID, STT) "
                        + " VALUES(@TenTaiLieu, @NoiDungTaiLieu, @ThuMucID, @STT)";

            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("TenTaiLieu", model.TenTaiLieu));
            listParam.Add(new SqlParameter("NoiDungTaiLieu", model.NoiDungTaiLieu));
            listParam.Add(new SqlParameter("ThuMucID", model.ThuMucID));
            listParam.Add(new SqlParameter("STT", model.STT));
            resultEvent = ExecuteNonQuery(strCommand, listParam.ToArray());
            if (resultEvent.Error != 0)
            {
                Log.ErrorFormat("Error: {0}", resultEvent.ErrorMessage);
                return -1;
            }
            return resultEvent.Result;
        }

        public int InsertTaiLieu(DetailTaiLieuModel model,List<HttpPostedFileBase> files, String filePath)
        {
            ResultEvent result = new ResultEvent();
            if (string.IsNullOrEmpty(_strConnect))
            {
                result.SetError(_connectStringNULL);
                return result.Result;
            }
            SqlTransaction myTransaction;
            using (SqlConnection conn = new SqlConnection(_strConnect))
            {
                int idTaiLieu = 0;
                conn.Open();
                if (conn.State != ConnectionState.Open)
                {
                    result.SetError(_cannotConnectToServer);
                    return result.Result;
                }
                myTransaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    idTaiLieu = InsertTaiLieu(conn, myTransaction, model);
                    if (files != null)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            HttpPostedFileBase file = files[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                var fileName =  file.FileName;
                                var path = Path.Combine(filePath, fileName);
                                file.SaveAs(path);
                                InsertFiles(conn, myTransaction, idTaiLieu, fileName, file.ContentType);
                            }
                        }
                    }
                    myTransaction.Commit();
                    result.Result = 1;
                }
                catch (Exception ex)
                {
                    myTransaction.Rollback();
                    result.SetError(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
                return result.Result;
            }
        }

        private int InsertTaiLieu(SqlConnection conn, SqlTransaction trans, DetailTaiLieuModel model)
        {
            String strCommand = "INSERT INTO tailieu (TenTaiLieu, NoiDungTaiLieu, ThuMucID, STT) output INSERTED.TaiLieuID "
                        + " VALUES(@TenTaiLieu, @NoiDungTaiLieu, @ThuMucID, @STT)";

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strCommand;
            cmd.Transaction = trans;

            cmd.Parameters.Add("TenTaiLieu", SqlDbType.NVarChar).Value = (object)model.TenTaiLieu ?? DBNull.Value;
            cmd.Parameters.Add("NoiDungTaiLieu", SqlDbType.NVarChar).Value = (object)model.NoiDungTaiLieu ?? DBNull.Value;
            cmd.Parameters.Add("ThuMucID", SqlDbType.Int).Value = (object)model.ThuMucID ?? DBNull.Value;
            cmd.Parameters.Add("STT", SqlDbType.Int).Value = (object)model.STT ?? DBNull.Value;

            int modified = (int)cmd.ExecuteScalar();

            return modified;

        }

        private int InsertFiles(SqlConnection conn, SqlTransaction trans, int taiLieuId, String fileName, String fileType)
        {
            String strCommand = "INSERT INTO FILES (TaiLieuId, FileName, FileType) "
                        + " VALUES(@TaiLieuId, @FileName, @FileType)";

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strCommand;
            cmd.Transaction = trans;

            cmd.Parameters.Add(new SqlParameter("TaiLieuId", taiLieuId));
            cmd.Parameters.Add(new SqlParameter("FileName", fileName));
            cmd.Parameters.Add(new SqlParameter("FileType", fileType));

            return cmd.ExecuteNonQuery();

        }

        public int UpdateTaiLieu(DetailTaiLieuModel model)
        {
            ResultEvent resultEvent = new ResultEvent();
            String strCommand = "UPDATE tailieu "
                        + " SET TenTaiLieu = @TenTaiLieu"
                        + " , NoiDungTaiLieu = @NoiDungTaiLieu"
                        + " , ThuMucID = @ThuMucID"
                        + " , STT = @STT"
                        + " WHERE TaiLieuID = @TaiLieuID";

            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("TenTaiLieu", model.TenTaiLieu));
            listParam.Add(new SqlParameter("NoiDungTaiLieu", model.NoiDungTaiLieu));
            listParam.Add(new SqlParameter("ThuMucID", model.ThuMucID));
            listParam.Add(new SqlParameter("STT", model.STT));
            listParam.Add(new SqlParameter("TaiLieuID", model.TaiLieuID));
            resultEvent = ExecuteNonQuery(strCommand, listParam.ToArray());
            if (resultEvent.Error != 0)
            {
                Log.ErrorFormat("Error: {0}", resultEvent.ErrorMessage);
                return -1;
            }
            return resultEvent.Result;
        }

        public ResultEvent GetTaiLieuByThuMuc(int thumucid)
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" SELECT ");
            strCommand.AppendLine("     tl.TaiLieuID");
            strCommand.AppendLine("     ,tl.TenTaiLieu");
            strCommand.AppendLine("     ,tl.ThuMucID");
            strCommand.AppendLine("     ,tl.stt");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     TaiLieu tl");
            strCommand.AppendLine(" WHERE ");
            strCommand.AppendLine("     tl.ThuMucID = @ThuMucID");
            strCommand.AppendLine(" ORDER BY ");
            strCommand.AppendLine("     tl.stt");
            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("ThuMucID", thumucid));
            resultEvent = ExecuteMyQuery(strCommand.ToString(), listParam.ToArray());

            return resultEvent;
        }

        public ResultEvent GetDetailTaiLieuById(int tailieuid)
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" SELECT ");
            strCommand.AppendLine("     tl.TaiLieuID");
            strCommand.AppendLine("     ,tl.TenTaiLieu");
            strCommand.AppendLine("     ,tl.NoiDungTaiLieu");
            strCommand.AppendLine("     ,tl.ThuMucID");
            strCommand.AppendLine("     ,tl.STT");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     TaiLieu tl");
            strCommand.AppendLine(" WHERE ");
            strCommand.AppendLine("     tl.TaiLieuID = @TaiLieuID");
            strCommand.AppendLine(" ORDER BY ");
            strCommand.AppendLine("     tl.stt");
            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("TaiLieuID", tailieuid));
            resultEvent = ExecuteMyQuery(strCommand.ToString(), listParam.ToArray());

            return resultEvent;
        }


        public ResultEvent GetFileByTaiLieu(int tailieuid)
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" SELECT ");
            strCommand.AppendLine("     fi.TaiLieuID");
            strCommand.AppendLine("     ,fi.FileName");
            strCommand.AppendLine("     ,fi.FileType");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     Files fi");
            strCommand.AppendLine(" WHERE ");
            strCommand.AppendLine("     fi.TaiLieuID = @TaiLieuID");
            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("TaiLieuID", tailieuid));
            resultEvent = ExecuteMyQuery(strCommand.ToString(), listParam.ToArray());

            return resultEvent;
        }
        #endregion

        #region Thư mục
        public ResultEvent GetAllThuMuc()
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" SELECT ");
            strCommand.AppendLine("     tm.thumucid");
            strCommand.AppendLine("     ,tm.tenthumuc");
            strCommand.AppendLine("     ,tm.thumucchaid");
            strCommand.AppendLine("     ,tmparent.tenthumuc as TenThuMucCha");
            strCommand.AppendLine("     ,tm.stt");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     ThuMucTaiLieu tm");
            strCommand.AppendLine(" LEFT JOIN ThuMucTaiLieu tmparent");
            strCommand.AppendLine(" ON tmparent.thumucid = tm.thumucchaid");
            strCommand.AppendLine(" ORDER BY ");
            strCommand.AppendLine("     tm.stt");
            List<SqlParameter> listParam = new List<SqlParameter>();
            resultEvent = ExecuteMyQuery(strCommand.ToString(), listParam.ToArray());

            return resultEvent;
        }

        public ResultEvent GetUnionThuMuc1()
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" WITH DirectReports (ThuMucID, ThuMucChaID, TenThuMucCha, TenThuMuc, ThuMucLevel, STT) ");
            strCommand.AppendLine("   AS ( SELECT e.ThuMucID,");
            strCommand.AppendLine("                 e.ThuMucChaID,");
            strCommand.AppendLine("                 parent.TenThuMuc as TenThuMucCha,");
            strCommand.AppendLine("                 e.TenThuMuc,");
            strCommand.AppendLine("                 1 AS ThuMucLevel,");
            strCommand.AppendLine("                 e.STT");
            strCommand.AppendLine("         FROM ThuMucTaiLieu e");
            strCommand.AppendLine("         INNER JOIN ThuMucTaiLieu parent");
            strCommand.AppendLine("         ON parent.ThuMucID = e.ThuMucChaID");
            strCommand.AppendLine("         WHERE e.ThuMucChaID = e.ThuMucID ");
            strCommand.AppendLine("         UNION ALL");
            strCommand.AppendLine("         SELECT  e.ThuMucID,");
            strCommand.AppendLine("                 e.ThuMucChaID,");
            strCommand.AppendLine("                 parent.TenThuMuc as TenThuMucCha,");
            strCommand.AppendLine("                 e.TenThuMuc,");
            strCommand.AppendLine("                 d.ThuMucLevel + 1 AS ThuMucLevel,");
            strCommand.AppendLine("                 e.STT");
            strCommand.AppendLine("         FROM ThuMucTaiLieu e");
            strCommand.AppendLine("         INNER JOIN ThuMucTaiLieu parent");
            strCommand.AppendLine("         ON parent.ThuMucID = e.ThuMucChaID");
            strCommand.AppendLine("         INNER JOIN DirectReports d ");
            strCommand.AppendLine("         ON d.ThuMucID = e.ThuMucChaID");
            strCommand.AppendLine("         AND e.ThuMucChaID != e.ThuMucID )");
            strCommand.AppendLine(" SELECT ThuMucID, REPLIcate(' + ', (ThuMucLevel - 1)) + TenThuMuc AS TenThuMuc,ThuMucChaID, ThuMucLevel, STT");
            strCommand.AppendLine(" FROM DirectReports ORDER BY ThuMucLevel, STT");

            List<SqlParameter> listParam = new List<SqlParameter>();
            resultEvent = ExecuteMyQuery(strCommand.ToString(), listParam.ToArray());

            return resultEvent;
        }

        public ResultEvent GetDetailThuMucById(int thumucid)
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" SELECT ");
            strCommand.AppendLine("     tm.thumucid");
            strCommand.AppendLine("     ,tm.tenthumuc");
            strCommand.AppendLine("     ,tm.thumucchaid");
            strCommand.AppendLine("     ,tm.stt");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     ThuMucTaiLieu tm");
            strCommand.AppendLine(" WHERE ");
            strCommand.AppendLine("     tm.thumucid = @thumucid");
            strCommand.AppendLine(" ORDER BY ");
            strCommand.AppendLine("     tm.stt");
            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("thumucid", thumucid));
            resultEvent = ExecuteMyQuery(strCommand.ToString(), listParam.ToArray());

            return resultEvent;
        }

        public int InsertThuMuc(ThuMucModel model)
        {
            ResultEvent resultEvent = new ResultEvent();
            String strCommand = "INSERT INTO ThuMucTaiLieu (TenThuMuc, ThuMucChaID, STT) "
                        + " VALUES(@TenThuMuc, @ThuMucChaID, @STT)";

            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("TenThuMuc", model.TenThuMuc));
            listParam.Add(new SqlParameter("ThuMucChaID", model.ThuMucChaID));
            listParam.Add(new SqlParameter("STT", model.STT));
            resultEvent = ExecuteNonQuery(strCommand, listParam.ToArray());
            if (resultEvent.Error != 0)
            {
                Log.ErrorFormat("Error: {0}", resultEvent.ErrorMessage);
                return -1;
            }
            return resultEvent.Result;
        }

        public int UpdateThuMuc(ThuMucModel model)
        {
            ResultEvent resultEvent = new ResultEvent();
            String strCommand = "UPDATE ThuMucTaiLieu "
                        + " SET TenThuMuc = @TenThuMuc"
                        + " , ThuMucChaID = @ThuMucChaID"
                        + " , STT = @STT"
                        + " WHERE ThuMucID = @ThuMucID";

            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("TenThuMuc", model.TenThuMuc));
            listParam.Add(new SqlParameter("ThuMucChaID", model.ThuMucChaID));
            listParam.Add(new SqlParameter("STT", model.STT));
            listParam.Add(new SqlParameter("ThuMucID", model.ThuMucID));
            resultEvent = ExecuteNonQuery(strCommand, listParam.ToArray());
            if (resultEvent.Error != 0)
            {
                Log.ErrorFormat("Error: {0}", resultEvent.ErrorMessage);
                return -1;
            }
            return resultEvent.Result;
        }

        #endregion

        public ResultEvent CheckTaiKhoanAdmin(string taikhoan)
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" SELECT ");
            strCommand.AppendLine("     tk.id_admin");
            strCommand.AppendLine("     ,tk.tai_khoan_admin ");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     TaiKhoanAdmin tk ");
            strCommand.AppendLine(" WHERE ");
            strCommand.AppendLine("     tk.is_delete = 0");
            strCommand.AppendLine("     AND tk.tai_khoan_admin = @tai_khoan_admin");
            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("tai_khoan_admin", taikhoan));
            resultEvent = ExecuteMyQuery(strCommand.ToString(), listParam.ToArray());

            return resultEvent;
        }

        public int DeleteThuMuc(int thumucid)
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" DELETE ");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     ThuMucTaiLieu ");
            strCommand.AppendLine(" WHERE ");
            strCommand.AppendLine("      thumucid = @thumucid");

            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("thumucid", thumucid));
            resultEvent = ExecuteNonQuery(strCommand.ToString(), listParam.ToArray());
            if (resultEvent.Error != 0)
            {
                Log.ErrorFormat("Error: {0}", resultEvent.ErrorMessage);
                return -1;
            }
            return resultEvent.Result;
        }

        public int DeleteTaiLieu(int taiLieuID)
        {
            ResultEvent resultEvent = new ResultEvent();
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(" DELETE ");
            strCommand.AppendLine(" FROM ");
            strCommand.AppendLine("     Tailieu ");
            strCommand.AppendLine(" WHERE ");
            strCommand.AppendLine("      TaiLieuID = @TaiLieuID");

            List<SqlParameter> listParam = new List<SqlParameter>();
            listParam.Add(new SqlParameter("TaiLieuID", taiLieuID));
            resultEvent = ExecuteNonQuery(strCommand.ToString(), listParam.ToArray());
            if (resultEvent.Error != 0)
            {
                Log.ErrorFormat("Error: {0}", resultEvent.ErrorMessage);
                return -1;
            }
            return resultEvent.Result;
        }

    }
}