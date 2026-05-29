using System;
using System.Net;

namespace SubnetCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Введите IP-адрес сети (например, 192.168.1.0): ");
                string ipInput = Console.ReadLine();
                IPAddress ip = IPAddress.Parse(ipInput);

                Console.Write("Введите исходную маску подсети (в формате CIDR, например, 24 для 255.255.255.0): ");
                int cidr = int.Parse(Console.ReadLine());

                Console.Write("Введите желаемое количество подсетей: ");
                int requiredSubnets = int.Parse(Console.ReadLine());

                // Определяем количество бит, необходимых для заданного числа подсетей
                int bitsBorrowed = (int)Math.Ceiling(Math.Log(requiredSubnets, 2));
                int newCidr = cidr + bitsBorrowed;

                if (newCidr > 30) // /31 и /32 не используются для стандартных диапазонов хостов
                {
                    Console.WriteLine("Ошибка: Для такого количества подсетей не хватит адресов хостов.");
                    return;
                }

                // Реальное количество подсетей (всегда степень двойки)
                int actualSubnets = (int)Math.Pow(2, bitsBorrowed);
                uint ipAsUint = IpToUint(ip);

                // Количество адресов в одной подсети
                int hostBits = 32 - newCidr;
                uint addressesPerSubnet = (uint)Math.Pow(2, hostBits);

                Console.WriteLine("\n=== Результаты ===");
                Console.WriteLine($"Новая маска подсети: /{newCidr}");
                Console.WriteLine($"Фактическое количество созданных подсетей: {actualSubnets}\n");

                for (int i = 0; i < actualSubnets; i++)
                {
                    uint networkAddress = ipAsUint + (uint)(i * addressesPerSubnet);
                    uint broadcastAddress = networkAddress + addressesPerSubnet - 1;

                    uint firstHost = networkAddress + 1;
                    uint lastHost = broadcastAddress - 1;

                    Console.WriteLine($"[Подсеть {i + 1}]");
                    Console.WriteLine($"  Адрес сети:          {UintToIp(networkAddress)}");
                    Console.WriteLine($"  Диапазон хостов:     {UintToIp(firstHost)} - {UintToIp(lastHost)}");
                    Console.WriteLine($"  Broadcast:   {UintToIp(broadcastAddress)}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка ввода: {ex.Message}");
            }
        }

        // Конвертация IPAddress в 32-битное беззнаковое число
        static uint IpToUint(IPAddress ipAddress)
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            Array.Reverse(bytes); // Учитываем порядок байтов (Endianness)
            return BitConverter.ToUInt32(bytes, 0);
        }

        // Конвертация 32-битного числа обратно в строку IP-адреса
        static string UintToIp(uint ipValue)
        {
            byte[] bytes = BitConverter.GetBytes(ipValue);
            Array.Reverse(bytes);
            return new IPAddress(bytes).ToString();
        }
    }
}