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
    public class TaiKhoanAdminAccess : BaseAccess
    {
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
    }
}