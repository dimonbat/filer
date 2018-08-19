# filer
Collect files recursively from given path and compute this hashes and then write it to database

## Usage ##
Все управление программой заключается в редактировании файла настроек test.ini (должен находиться в том же каталоге что и бинарник) и указании параметров командной строки.    
Примерное содержимое test.ini:
    
    [database]
    host=localhost
    instance=SQLEXPRESS
    database=MyDatabase5
    
    [directory]
    startdir=D:\f-a
    

здесь:

    host - имя хоста с базой данных MsSql
    instance - инстанс базы данных
    database - имя базы данных
    startdir - начальная директория для обхода

ключи:

    -с - создать указанную в конфиге базу данных (и таблицу files в ней)
    без ключей - обойти каталог startdir и его подкалалоги, собрать файлы, посчитать хэши, и записать результат в указанную базу данных (используется таблица files)
    
## ToDO ##
- Нужно сделать нормальный обход подкаталогов вручную, в данный момент используется dir.GetDirectories("*.*", System.IO.SearchOption.AllDirectories), который ничего не выдаст при проблемах с доступом хотя бы к одному файлу.
- обернуть критичные части кода в try...except
- задать имя таблицы из конфига
- и еще много всякого разного
    

