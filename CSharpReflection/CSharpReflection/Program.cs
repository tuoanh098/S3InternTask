[Custom]
public class ReflectionInformation
{
    [Custom]
    private int _id;

    private string _name;

    [Custom]
    public ReflectionInformation()
    {

    }

    public ReflectionInformation(int id)
    {
        Id = id;
    }

    public ReflectionInformation(int id, string name)
    {
        Id = id;
        Name = name;
    }

    [Custom]
    public int Id { get; set; }
    public string Name { get; set; }

    [Custom]
    public void Write()
    {
        Console.WriteLine("Id: " + Id);
        Console.WriteLine("Name: " + Name);
    }

    public void Write(string name)
    {
        Console.WriteLine("Name: " + name);
    }
}
public interface IValidate
{
    bool IsOk(string text);
}
public class TextNotEmpty : IValidate
{
    public bool IsOk(string text)
    {
        return !string.IsNullOrEmpty(text);
    }
}
public class TextAtLeast8Chars : IValidate
{
    public bool IsOk(string text)
    {
        return text.Length >= 8;
    }
}
public class CustomAttribute : Attribute
{
    public string Name { get; set; }

    public void Write()
    {
        Console.WriteLine("Hello CustomAttribute.");
    }
}
public class ReflectionCSharp
{
    public void Run()
    {
        var reflectionInfo = new ReflectionInformation();
        var type = reflectionInfo.GetType();
        var typeV = typeof(IValidate);

        /*1. Initiate new object*/
        var firstRe = (ReflectionInformation)Activator.CreateInstance(type, new object[] { 10 });// Initiate new object with one parameter
        var secRe = (ReflectionInformation)Activator.CreateInstance(type, new object[] { 10, "Oanh" });//Initiate new object with two parameter
        firstRe.Write();
        secRe.Write();

        ///*2. Identity class*/
        var needValids = AppDomain.CurrentDomain.GetAssemblies()
                                        .SelectMany(s => s.GetTypes())
                                        .Where(p => typeV.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        var text = string.Empty;
        foreach (var item in needValids)
        {
            var o = Activator.CreateInstance(item, null) as IValidate;
            var ok = o.IsOk(text);

            Console.WriteLine(item + "==" + text + "==" + ok);
        }
        Console.WriteLine("==================");

        text = "WTF";
        foreach (var item in needValids)
        {
            var o = Activator.CreateInstance(item, null) as IValidate;
            var ok = o.IsOk(text);

            Console.WriteLine(item + "==" + text + "==" + ok);
        }
        Console.WriteLine("==================");

        text = "WTF WTF WTF";
        foreach (var item in needValids)
        {
            var o = Activator.CreateInstance(item, null) as IValidate;
            var ok = o.IsOk(text);

            Console.WriteLine(item + "==" + text + "==" + ok);
        }
    }
}

class Program
{ 
    static void Main(string[] args)
    {
        var re = new ReflectionCSharp();
        var type = re.GetType();
                                                                                               //
        re.Run();
        Console.ReadKey();
    }
}