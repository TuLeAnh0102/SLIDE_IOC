using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SlideIOC.DataAccess
{
    public class BaseAccess
    {
        protected const string _connectStringNULL = "ConnectString is Null";
        protected const string _cannotConnectToServer = "Can't open connect to server";
        private SqlTransaction Transaction;

        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string _strConnect = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        protected ResultEvent ExecuteMyQuery(string strCommand, params SqlParameter[] arrPara)
        {
            ResultEvent result = new ResultEvent();
            DataTable dt = new DataTable();

            if (string.IsNullOrEmpty(_strConnect))
            {
                if (_strConnect == null)
                {
                    result.SetError(_connectStringNULL);
                    return result;
                }
            }
            using (SqlConnection con = new SqlConnection(_strConnect))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(strCommand);
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;

                    con.Open();
                    //check connect
                    if (con.State != ConnectionState.Open)
                    {
                        result.SetError(_cannotConnectToServer);
                        return result;
                    }
                    //add paramters
                    foreach (SqlParameter param in arrPara)
                    {
                        cmd.Parameters.Add(param);

                    }
                    //map list result to datatable 
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    result.DataResult = dt;
                    result.Result = dt.Rows.Count;

                    return result;

                }
                catch (SqlException ex)
                {
                    result.SetError(ex.Message);
                }
                catch (Exception ex)
                {
                    result.SetError(ex.Message);
                }
                finally
                {
                    con.Close();
                }
                return result;
            }
        }

        protected ResultEvent ExecuteScalar(string strCommand, params SqlParameter[] arrPara)
        {
            ResultEvent result = new ResultEvent();
            if (string.IsNullOrEmpty(_strConnect))
            {
                result.SetError(_connectStringNULL);
                return result;
            }
            using (SqlConnection con = new SqlConnection(_strConnect))
            {
                try
                {
                    con.Open();
                    if (con.State != ConnectionState.Open)
                    {
                        result.SetError(_cannotConnectToServer);
                        return result;
                    }

                    Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = strCommand;
                    cmd.Transaction = Transaction;

                    foreach (SqlParameter param in arrPara)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                        cmd.Parameters.Add(param);
                    }

                    object count = cmd.ExecuteScalar();
                    result.Result = Decimal.ToInt32((decimal)count);

                    Transaction.Commit();
                }
                catch (SqlException ex)
                {
                    Transaction.Rollback();
                    result.SetError(ex.Message);
                }
                catch (Exception ex)
                {
                    Transaction.Rollback();
                    result.SetError(ex.Message);
                }
                finally
                {
                    con.Close();
                }
                return result;
            }
        }

        protected ResultEvent ExecuteNonQuery(string strCommand, params SqlParameter[] arrPara)
        {
            ResultEvent result = new ResultEvent();
            if (string.IsNullOrEmpty(_strConnect))
            {
                result.SetError(_connectStringNULL);
                return result;
            }
            using (SqlConnection con = new SqlConnection(_strConnect))
            {
                try
                {
                    con.Open();
                    if (con.State != ConnectionState.Open)
                    {
                        result.SetError(_cannotConnectToServer);
                        return result;
                    }

                    Transaction = con.BeginTransaction(IsolationLevel.ReadCommitted);

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = strCommand;
                    cmd.Transaction = Transaction;

                    foreach (SqlParameter param in arrPara)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                        cmd.Parameters.Add(param);

                    }
                    result.Result = cmd.ExecuteNonQuery();

                    if (result.Result == 0)
                    {
                        result.SetError("No Record Effected");
                    }
                    else
                    {
                        Transaction.Commit();
                    }
                }
                catch (SqlException ex)
                {
                    Transaction.Rollback();
                    result.SetError(ex.Message);
                }
                catch (Exception ex)
                {
                    Transaction.Rollback();
                    result.SetError(ex.Message);
                }
                finally
                {
                    con.Close();
                }
                return result;
            }
        }
    }

    public class ResultEvent
    {

        public int Result { get; set; }
        public string ErrorMessage { get; set; }

        private long _error = 0;
        public long Error
        {
            get { return _error; }
            set { this._error = value; }
        }

        private DataSet _dataSet = new System.Data.DataSet();
        public DataSet DataSet
        {
            get { return _dataSet; }
            set { this._dataSet = value; }
        }

        private DataTable _dtResult = new System.Data.DataTable();
        public DataTable DataResult
        {
            get { return _dtResult; }
            set { this._dtResult = value; }
        }

        public virtual void SetError(string errMessage)
        {
            this.Error = 1;
            this.ErrorMessage = errMessage;
        }
    }


}