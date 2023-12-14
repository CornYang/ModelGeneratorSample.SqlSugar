using CommandLine;
using PluralizeService.Core;
using SqlSugar;

namespace Model.Generator.SqlSugar
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<OptionSetting>(args).WithParsed(Run);
        }

        private static void Run(OptionSetting option)
        {
            using var _db = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = DbType.SqlServer,
                ConnectionString = option.ConnectionStr,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            });
            foreach (var item in _db.DbMaintenance.GetTableInfoList())
            {
                string entityName = item.Name.ToPascalCase().Pluralize();
                _db.MappingTables.Add(entityName, item.Name);
                foreach (var col in _db.DbMaintenance.GetColumnInfosByTableName(item.Name))
                {
                    _db.MappingColumns.Add(col.DbColumnName.ToPascalCase(), col.DbColumnName, entityName);
                }
            }
            _db.DbFirst.IsCreateAttribute().CreateClassFile(option.ModelPath, option.ModelNameSpace);
        }
    }

    public class OptionSetting
    {
        [Option('c', "connectionstring", Required = true, HelpText = "資料庫連線字串")]
        public string? ConnectionStr { get; set; }

        [Option('p', "path", Required = true, HelpText = "Models檔案放置路徑")]
        public string? ModelPath { get; set; }

        [Option('n', "namespace", Required = true, HelpText = "Model命名空間")]
        public string? ModelNameSpace { get; set; }
    }

    public static class StringExtension
    {
        public static string ToPascalCase(this string value)
        {
            var words = value.Split(new[] { "_", "-", " " }, StringSplitOptions.RemoveEmptyEntries);
            words = words
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .ToArray();
            return string.Join(string.Empty, words);
        }

        public static string Pluralize(this string value)
        {
            return PluralizationProvider.Pluralize(value);
        }
    }
}
