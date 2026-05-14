using Scriban;
using System.Text.Json;

var json = File.ReadAllText(@"C:\Projekte\ReportService\employee_purchase_test.json");
var data = JsonSerializer.Deserialize<object>(json);

var templateText = @"
Name: {{ EmployeeName }}
Number: {{ EmployeeNumber }}
Items:
{{ for item in Items }}
- {{ item.Name }}: {{ item.Quantity }}
{{ end }}
";

var template = Template.Parse(templateText);
var result = template.Render(data);

Console.WriteLine(result);
Console.WriteLine("\n\nObject type: " + data?.GetType().Name);
