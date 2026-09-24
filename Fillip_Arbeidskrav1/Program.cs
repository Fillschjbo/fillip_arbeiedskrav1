using System;
using System.IO;

namespace Fillip_Arbeidskrav1;

class Program
{
    static void Main(string[] args)
    {
        string phonebookPath = Path.Combine(AppContext.BaseDirectory, "data", "phonebook.csv");

        Phonebook phonebook;
        try
        { 
            phonebook = Phonebook.Load(phonebookPath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load phone book: {e.Message}");
            return;
        }
        
        Console.WriteLine("--- 1. Linear search, unsorted ---");
        Console.WriteLine($"{"Field",-12}{"target",-15}{"Matches",-10}{"Comparisons",-12}");
        
        RunLinearSearch(phonebook, Field.Lastname, "Bjerke");
        RunLinearSearch(phonebook, Field.Lastname, "Mathisen");
        RunLinearSearch(phonebook, Field.Lastname, "Husebø");
        RunLinearSearch(phonebook, Field.Mobile, "0000000");
    }

    static void RunLinearSearch(Phonebook phonebook, Field field, string target)
    {
        Contact[] results = phonebook.LinearSearch(field, target);
        Console.WriteLine($"{field,-12}{target,-15}{results.Length,-10}{phonebook.Comparisons,-12}");
    }
}