# File_System_with_iNodes_Simulation
Tanenbaum_4_48 Modern OS
48. Создайте смоделированную файловую систему, которая полностью помещалась бы в отдельный обычный файл, сохраненный на диске. В этом дисковом файле должны содержаться каталоги, i-узлы, информация о свободных блоках, блоки файловых данных и т. д. Выберите подходящий алгоритм для сохранения ин- формации о свободных блоках и для размещения блоков данных (непрерывного, индексированного, связанного). Ваша программа должна воспринимать посту- пающие от пользователя системные команды на создание и удаление каталогов, создание, удаление и открытие файлов, чтение выбранного файла и записи его на диск, а также вывод содержимого каталога.