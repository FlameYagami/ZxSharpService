using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using ZxWrapper.Entity;

namespace ZxSharpService
{
    public class CardManager
    {
        public const string ColumnMd5 = "Md5";
        public const string ColumnType = "Type";
        public const string ColumnCamp = "Camp";
        public const string ColumnRace = "Race";
        public const string ColumnSign = "Sign";
        public const string ColumnCost = "Cost";
        public const string ColumnPower = "Power";
        public const string DatabaseName = "Data.db";
        public static string DatabasePath = $"Data Source='{AppDomain.CurrentDomain.BaseDirectory + DatabaseName}'";

        private static Dictionary<string, Card> _mCardEntities;

        public static void Init()
        {
            if (null == _mCardEntities)
            {
                _mCardEntities = new Dictionary<string, Card>();
            }
            var connection = new SQLiteConnection(DatabasePath);
            connection.Open();
            var command = new SQLiteCommand("select Md5,Type,Camp,Race,Sign,Power,Cost from TableCard where number in (select max(number) from TableCard group by Md5)", connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var md5= reader[ColumnMd5].ToString();
                var cardEntity = new Card()
                {
                    Md5 = reader[ColumnMd5].ToString(),
                    Type = reader[ColumnType].ToString(),
                    Camp = reader[ColumnCamp].ToString(),
                    Race = reader[ColumnRace].ToString(),
                    Sign = reader[ColumnSign].ToString(),
                    Cost = reader[ColumnCost].ToString(),
                    Power = reader[ColumnPower].ToString()
                };
                _mCardEntities.Add(md5, cardEntity);
            }
            reader.Close();
            connection.Close();
            Logger.WriteLine(_mCardEntities.Count);
        }

         public static Card GetCard(string md5)
         {
             return _mCardEntities.ContainsKey(md5) ? _mCardEntities[md5] : null;
         }
    }
}
