using Service.Models;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Service.Modules
{
    public static class DbUtils
    {
        private static string[] getConfigs(IConfiguration config)
        {
            return new[] { config["ConnectionStrings:Database1"], config["ConnectionStrings:Database2"], config["ConnectionStrings:Database3"], config["ConnectionStrings:Database4"], config["ConnectionStrings:Database5"] };
        }

        public static void InitDB(IConfiguration config)
        {
            var ConnStrs = getConfigs(config);
            for (int i = 0; i < ConnStrs.Length; i++)
            {
                string ConnStr = ConnStrs[i];
                if (!string.IsNullOrWhiteSpace(ConnStr))
                {
                    try
                    {
                        Debug.WriteLine(ConnStr);
                        using (SqlConnection con = new SqlConnection(ConnStr))
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(@"IF DB_ID('BookStore') IS NULL CREATE DATABASE BookStore;", con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            con.Close();
                        }
                        using (SqlConnection con = new SqlConnection(ConnStr.Replace("master", "BookStore")))
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(@"IF OBJECT_ID('Books', 'U') IS NULL CREATE TABLE Books ([id] [varchar](50) NOT NULL,[BookName] [nvarchar](1000) NULL,[Category] [nvarchar](100) NULL,[Author] [nvarchar](100) NULL,[Price] [decimal](15, 2) NULL, [Sync] [bit] NULL, [Deleted] [bit] NULL);", con))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            con.Close();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }



        public static List<MsBook> GetBooks(IConfiguration config)
        {
            var ConnStrs = getConfigs(config);
            var list = new List<MsBook>();
            for (int i = 0; i < ConnStrs.Length; i++)
            {
                string ConnStr = ConnStrs[i];
                if (!string.IsNullOrWhiteSpace(ConnStr))
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConnStr.Replace("master", "BookStore")))
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM Books WHERE [Deleted] IS NULL", con))
                            {
                                using (SqlDataReader dr = cmd.ExecuteReader())
                                {
                                    while (dr.Read())
                                    {
                                        list.Add(new MsBook()
                                        {
                                            Id = dr["id"].ToString(),
                                            BookName = dr["BookName"]?.ToString(),
                                            Category = dr["Category"]?.ToString(),
                                            Author = dr["Author"]?.ToString(),
                                            Price = string.IsNullOrEmpty(dr["Price"]?.ToString()) ? null : Convert.ToDecimal(dr["Price"].ToString()),
                                        });
                                    }
                                    dr.Close();
                                    return list;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
            }
            return list;
        }

        public static MsBook GetBook(IConfiguration config, string id, string connStr = "")
        {
            var ConnStrs = string.IsNullOrWhiteSpace(connStr) ? getConfigs(config) : [connStr];
            for (int i = 0; i < ConnStrs.Length; i++)
            {
                string ConnStr = ConnStrs[i];
                if (!string.IsNullOrWhiteSpace(ConnStr) && !string.IsNullOrWhiteSpace(id))
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConnStr.Replace("master", "BookStore")))
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM Books WHERE [Id]=@pId AND [Deleted] IS NULL", con))
                            {
                                cmd.Parameters.AddWithValue("@pId", id ?? (object)DBNull.Value);
                                using (SqlDataReader dr = cmd.ExecuteReader())
                                {
                                    if (dr.Read())
                                    {
                                        return new MsBook()
                                        {
                                            Id = dr["id"].ToString(),
                                            BookName = dr["BookName"]?.ToString(),
                                            Category = dr["Category"]?.ToString(),
                                            Author = dr["Author"]?.ToString(),
                                            Price = string.IsNullOrEmpty(dr["Price"]?.ToString()) ? null : Convert.ToDecimal(dr["Price"].ToString()),
                                        };
                                    }
                                    dr.Close();
                                }
                            }
                            con.Close();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
            }
            return null;
        }

        public static bool AddBook(IConfiguration config, MsBook newBook, string connStr = "", string newid = "")
        {
            return AddBothBook(config, newBook, () => true, connStr, newid);
        }

        public static bool AddBothBook(IConfiguration config, MsBook newBook, Func<bool> doPart, string connStr = "", string newid = "")
        {
            string id = string.IsNullOrWhiteSpace(newid) ? Guid.NewGuid().ToString() : newid;
            string ConnStr = string.IsNullOrWhiteSpace(connStr) ? getConfigs(config)[0] : connStr;
            if (!string.IsNullOrWhiteSpace(ConnStr))
                try
                {
                    using (SqlConnection con = new SqlConnection(ConnStr.Replace("master", "BookStore")))
                    {
                        con.Open();
                        SqlTransaction transaction = string.IsNullOrWhiteSpace(newid) ? con.BeginTransaction() : null;

                        using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Books([id], [BookName], [Category], [Author], [Price], [Sync]) VALUES (@pId, @pBookName, @pCategory, @pAuthor, @pPrice, 1)", con))
                        {
                            cmd.Parameters.AddWithValue("@pId", id);
                            cmd.Parameters.AddWithValue("@pBookName", newBook.BookName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pCategory", newBook.Category ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pAuthor", newBook.Author ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pPrice", newBook.Price ?? (object)DBNull.Value);
                            if (string.IsNullOrWhiteSpace(newid))
                                cmd.Transaction = transaction;
                            cmd.ExecuteNonQuery();
                        }

                        if (string.IsNullOrWhiteSpace(newid))
                        {
                            if (doPart())
                                transaction.Commit();
                            else transaction.Rollback();
                        }
                        con.Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            return false;

        }

        public static bool UpdateBook(IConfiguration config, string id, MsBook newBook, string connStr = "")
        {
            string ConnStr = string.IsNullOrWhiteSpace(connStr) ? getConfigs(config)[0] : connStr;
            if (!string.IsNullOrWhiteSpace(ConnStr) && !string.IsNullOrWhiteSpace(id))
                try
                {
                    using (SqlConnection con = new SqlConnection(ConnStr.Replace("master", "BookStore")))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(@"UPDATE Books SET [BookName]=@pBookName, [Category]=@pCategory, [Author]=@pAuthor, [Price]=@pPrice, [Sync]=1 WHERE [Id]=@pId", con))
                        {
                            cmd.Parameters.AddWithValue("@pBookName", newBook.BookName ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pCategory", newBook.Category ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pAuthor", newBook.Author ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pPrice", newBook.Price ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@pId", id ?? (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            return false;
        }

        public static bool DeleteBook(IConfiguration config, string id, string connStr = "")
        {
            string ConnStr = string.IsNullOrWhiteSpace(connStr) ? getConfigs(config)[0] : connStr;
            if (!string.IsNullOrWhiteSpace(ConnStr) && !string.IsNullOrWhiteSpace(id))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(ConnStr.Replace("master", "BookStore")))
                    {
                        con.Open();
                        var cmdText = string.IsNullOrWhiteSpace(connStr)
                            ? @"UPDATE Books SET [Sync]=1, [Deleted]=1 WHERE [Id]=@pId"
                            : @"DELETE FROM Books WHERE [Id]=@pId";
                        using (SqlCommand cmd = new SqlCommand(cmdText, con))
                        {
                            cmd.Parameters.AddWithValue("@pId", id ?? (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public static void ReplicateData(IConfiguration config)
        {
            var ConnStrs = getConfigs(config);
            string ConnStr = ConnStrs[0];
            if (!string.IsNullOrWhiteSpace(ConnStr))
                try
                {
                    using (SqlConnection con = new SqlConnection(ConnStr.Replace("master", "BookStore")))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(@"SELECT * FROM Books WHERE [Sync]=1", con))
                        {
                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    var isDeleted = (dr["Deleted"] as bool? ?? false);
                                    var id = dr["id"].ToString();
                                    if (isDeleted)
                                    {
                                        var deleted = true;
                                        for (int i = 1; i < ConnStrs.Length; i++)
                                        {
                                            deleted = deleted && DeleteBook(config, id, ConnStrs[i]);
                                        }
                                        if (deleted)
                                            DeleteBook(config, id, ConnStrs[0]);
                                    }
                                    else
                                    {
                                        var book = new MsBook()
                                        {
                                            Id = id,
                                            BookName = dr["BookName"]?.ToString(),
                                            Category = dr["Category"]?.ToString(),
                                            Author = dr["Author"]?.ToString(),
                                            Price = string.IsNullOrEmpty(dr["Price"]?.ToString()) ? null : Convert.ToDecimal(dr["Price"].ToString()),
                                        };
                                        var success = true;
                                        for (int i = 1; i < ConnStrs.Length; i++)
                                        {
                                            var updated = GetBook(config, id, ConnStrs[i]);

                                            if (updated == null)
                                                success = success && AddBook(config, book, ConnStrs[i], id);
                                            else success = success && UpdateBook(config, id, book, ConnStrs[i]);

                                        }
                                        if (success)
                                        {
                                            using (SqlCommand cmd2 = new SqlCommand(@"UPDATE Books SET [Sync]=0 WHERE [Id]=@pId", con))
                                            {
                                                cmd2.Parameters.AddWithValue("@pId", id ?? (object)DBNull.Value);
                                                cmd2.ExecuteNonQuery();
                                            }
                                        }
                                    }

                                }
                                dr.Close();
                            }
                        }

                        con.Close();
                    }
                }
                catch (Exception ex)
                {

                }

        }

    }
}
