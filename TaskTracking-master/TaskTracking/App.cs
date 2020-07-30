using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskTracking
{
    class App
    {
        const int pageLength = 25;
        const string filename = @"TaskList.txt";

        private int selectedTask = 0;

        private List<string> tasks = new List<string>();
        private List<bool> isActioned = new List<bool>();


        public App()
        {
            Console.OutputEncoding = Encoding.Unicode;

            ReadListFromFile();
        }


        // Top-Level program flow
        public void Run()
        {
            bool quit;

            do
            {
                RemoveFirstActionedItems();
                PrintTaskList();
                var key = PresentMainMenuAndReturnUserSelection();
                quit = HandleUserInput(key);
            } while (!quit);

            WriteListToFile();

            Console.WriteLine(); // Ensures "press any key to quit..." is on its own line
        }


        private bool HandleUserInput(ConsoleKey key)
        {
            var hasTasks = tasks.Any();

            switch (key)
            {
                case ConsoleKey.A:
                    InputTaskToList();
                    break;
                case ConsoleKey.D when hasTasks:
                    DeleteCurrentlySelectedTask();
                    break;
                case ConsoleKey.N when hasTasks:
                    SelectNextPage();
                    break;
                case ConsoleKey.DownArrow when hasTasks:
                    SelectNextUnactionedTask();
                    break;
                case ConsoleKey.Enter when hasTasks:
                    WorkOnSelectedTask();
                    break;
                case ConsoleKey.Q:
                    return true;
            }

            return false;
        }


        // Console UI
        private void PrintTaskList()
        {
            Console.Clear();

            var page = GetPage();
            var startingPoint = FirstElementInPage(page);
            int endingPoint = FirstElementInPage(page + 1);

            for (int i = startingPoint; (i < endingPoint) && (i < tasks.Count); ++i)
            {
                if (isActioned[i])
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                else if (i == selectedTask)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine(tasks[i]);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            Console.WriteLine();
        }


        private ConsoleKey PresentMainMenuAndReturnUserSelection()
        {
            ConsoleKey key;

            PrintUsageOptions();
            key = GetInputFromUser();

            return key;
        }


        private void WorkOnSelectedTask()
        {
            bool valid = false;

            do
            {
                Console.Clear();
                Console.WriteLine($"Working on: {tasks[selectedTask]}");
                Console.WriteLine("r: re-enter | c: complete | q: cancel");
                Console.Write("Input: ");

                var key = GetInputFromUser();

                switch (key)
                {
                    case ConsoleKey.R:
                        ReenterTask();
                        valid = true;
                        break;
                    case ConsoleKey.C:
                        DeleteCurrentlySelectedTask();
                        valid = true;
                        break;
                    case ConsoleKey.Q:
                        valid = true;
                        break;
                }
            } while (!valid);
        }


        private void PrintUsageOptions()
        {
            if (tasks.Any())
            {
                Console.WriteLine("a: add | d: delete | n: next page | \u2193: select | enter: action | q: quit");
            }
            else
            {
                Console.WriteLine("a: add | q: quit");
            }

            Console.Write("Input: ");
        }


        private void InputTaskToList()
        {
            Console.Clear();
            Console.Write("Add a new task (empty to cancel): ");

            var input = Console.ReadLine();

            AddTaskToList(input);
        }


        private ConsoleKey GetInputFromUser()
        {
            return Console.ReadKey().Key;
        }


        // Item list management
        private void AddTaskToList(string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                tasks.Add(input);
                isActioned.Add(false);
            }
        }


        private void DeleteCurrentlySelectedTask()
        {
            isActioned[selectedTask] = true;
            SelectNextUnactionedTask();
        }


        private void ReenterTask()
        {
            AddTaskToList(tasks[selectedTask]);
            DeleteCurrentlySelectedTask();
        }


        private void SelectNextUnactionedTask()
        {
            bool stop = false;
            int lastPage = GetPage();

            do
            {
                selectedTask += 1;

                if (selectedTask >= isActioned.Count)
                {
                    selectedTask = 0;
                    stop = true;
                }
                else
                {
                    var currentPage = GetPage();

                    if ((currentPage != lastPage) && AllItemsOnPageActioned(currentPage))
                    {
                        stop = true;
                    }
                    else
                    {
                        lastPage = currentPage;
                    }
                }
            } while (!stop && isActioned[selectedTask]);
        }


        private void RemoveFirstActionedItems()
        {
            while (isActioned.Any() && isActioned[0])
            {
                tasks.RemoveAt(0);
                isActioned.RemoveAt(0);
                selectedTask -= 1;
            }

            if (selectedTask < 0)
            {
                selectedTask = 0;
            }
        }


        // Page management
        private int GetPage()
        {
            return selectedTask / pageLength;
        }


        private void SelectNextPage()
        {
            var page = GetPage();
            selectedTask = LastElementInPage(page); 
            SelectNextUnactionedTask();
        }


        private bool AllItemsOnPageActioned(int page)
        {
            bool allActioned = true;

            for (int i = FirstElementInPage(page); i < FirstElementInPage(page + 1) && i < isActioned.Count; ++i)
            {
                allActioned &= isActioned[i];
            }

            return allActioned;
        }


        private static int FirstElementInPage(int page)
        {
            return page * pageLength;
        }


        private static int LastElementInPage(int page)
        {
            return FirstElementInPage(page + 1) - 1;
        }


        // File Handling
        private void ReadListFromFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    while (!sr.EndOfStream)
                    {
                        var input = sr.ReadLine();

                        var splits = input.Split(new char[] { '\x1e' });

                        if (splits.Length == 2)
                        {
                            tasks.Add(splits[0]);
                            isActioned.Add(bool.Parse(splits[1]));
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {; }
        }


        private void WriteListToFile()
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                for (int i = 0; i < tasks.Count; ++i)
                {
                    sw.WriteLine($"{tasks[i]}\x1e{isActioned[i]}");
                }
            }
        }
    }
}
