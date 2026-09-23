using System;
using System.IO;

namespace Fillip_Arbeidskrav1;

class Program
{
    static void Main(string[] args)
    {
        string phonebookPath = Path.Combine(AppContext.BaseDirectory, "data", "phonebook.csv");

        try
        {
            Phonebook phonebook = Phonebook.Load(phonebookPath);
            Console.WriteLine(phonebook.Count);
        }
        catch (FileNotFoundException e)
        {
            throw new FileNotFoundException("The file could not be found.", e);
        }
    }
}