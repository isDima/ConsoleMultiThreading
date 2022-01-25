using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
//using System.Threading.Tasks;


namespace _MultiThreading
{

    // обект для позиционирования печати в консоли используется делегат ParameterizedThreadStart для передачи параметров в поток
    public class Position
    {
        public int x; //координата по оси x
        public int y; //координата по оси y
    }

    internal class Program
    {
        private static readonly int countTread = 15;//количество потоков
        private static readonly int calculationLenght = 50; //длина расчетов
        private static readonly object lockerThread = new object(); //объект для блокировки потоков, нужен чтобы выводить по очереди в консоль 
        static int terminationNumber = 1; //порядковый номер завершения потока по факту
        static readonly Random delayTime = new Random(); // генератор случайных чисел используем как задержку вычислений
        static List<Thread> threads = new List<Thread>(); // список для хранения по токов, нужен для отслеживания потоков

        private static void Main(string[] args)
        {
            for (int i = 0; i < countTread; i++)
            {
                Position positionConsole = new Position(); // создадим объект для позиционирования консольного вывода
                positionConsole.y = i; // номер строки = номеру потока
                positionConsole.x = 8; // нечем с 8 - го символа печатать #
                Thread thread = new Thread(new ParameterizedThreadStart(Process)); //создаем поток передав внего обработчик
                thread.Name = $"{string.Format("{0:d2}", positionConsole.y+1)}";//назовем поток используя порядковый номер
                thread.Start(positionConsole);//стартуем поток передав в него объект position описанный классом Position (параметы x и у)
                threads.Add(thread); // добави поток в список
            }
            // В целом нижняя консрукция: ждет пока завершаться все потоки
            foreach (Thread thread in threads) //перебераем все потоки 
            {
                thread.Join(); //Блокирует вызывающий поток до тех пор, пока указанный поток не завершится
            }
            Console.SetCursorPosition(0, countTread); // спозиционируем курсор в строку threadCount
            Console.ForegroundColor = ConsoleColor.Green; // установи цвет в зеленый
            Console.WriteLine("All Done!");
            Console.ReadLine();
        }

        //Печатает индикатор выполнения (progress bar) 
        private static void DisplayBlock(int x, int y, int counterBlock, ConsoleColor color)
        {
            lock (lockerThread) // блокируем текущий поток чтобы вывести в консоль 
            {
                Console.ForegroundColor = color; // установим цвет символов в консоли
                Console.SetCursorPosition(x, y); // установим позицию курсора
                Console.Write($"# {string.Format("{0:d2}", counterBlock)}"); // печатаем индикатор - # и порядковый номер 
            }
        }

        //Имитация одного вычисления (задержка формируется случайным образм)
        private static bool Delay() 
        {
            try
            {
                
                int value = delayTime.Next(0, 10);  // генерируем случайным образом от 0 до 10
                Thread.Sleep(value * 500); //сгенерируемое чило умножи на 30 получив чило для усыпления потока
                value = 100 / value; //деление на 0 (вызовем исключение иметировав ошибку) 
                return true; //нет ошибки
            }
            catch //имитация Exception
            {
                return false; //возникла ошибка при делении на 0
            }
        }

        private static void Process(object obj)
        {
            //Извлекаем объект в котором хранятся координаты позиционирования курсора
            Position positionConsole = (Position)obj; // распаковываем объект как класс Position
            ConsoleColor colorBlock = ConsoleColor.Green; //цвет символов в зеленый
            var timerThread = new Stopwatch(); //создадим объект для определения времени выполнения потока 
            timerThread.Start(); // запускаем таймер отчета времени
            lock (lockerThread) // обрабатывается одним потоком в одно время (разделяем (лочим) потоки)
            {
                Console.ForegroundColor = colorBlock;
                // позиционируем курсор для печати
                Console.SetCursorPosition(0, positionConsole.y);
                //Выводим название (в нащем случае номер) и id потока
                Console.Write($"{Thread.CurrentThread.Name} ({string.Format("{0:d2}", Thread.CurrentThread.ManagedThreadId)}) ");
            }
            // имитация вычислений в потоке
            for (int i = 1; i <= calculationLenght; i++)
            {   //одна итерация выислений
                if (Delay()) // если при выислении нет ошибки, установим цвет символов в зеленый
                    colorBlock = ConsoleColor.Green;
                else
                    // если при выислении возникла ошибка, установим цвет символов в красный
                    colorBlock = ConsoleColor.Red;
                // на каждую итерацию i напечатаем блок для индикации процесса вычислений
                DisplayBlock(positionConsole.x, positionConsole.y, i, colorBlock);
                positionConsole.x++;  //сдвинем позицию в консоли на 1 (x+1)
                
            }
            lock (lockerThread) // блокируем поток для вывода в консоль
            {
                
                Console.ForegroundColor = ConsoleColor.Red; // цвет символов в красный
                Console.SetCursorPosition(positionConsole.x, positionConsole.y);// спозиционируем вывод в консоль
                Console.WriteLine($" {terminationNumber++}! ({timerThread.Elapsed.TotalSeconds})"); // выведем номер завершения потока и затраченное время
            }
        }
    }
}
