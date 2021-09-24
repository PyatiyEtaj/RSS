# Revit Socker Server [RSS]

Revit Socker Server - расширение для Revit на основе TcpListner. Является backend сервером и используется для гибкого расчета данных на bim360 файлах

# Revit Service

Revit Service - ASP NET frontend сервер взаимодействующий с Revit Socket Server `(RSS)`.

Назначение данного сервиса в передаче запросов от клиентов `(bim4bilders)` расширению для ревита, которое в свою очередь, используя ранее разработанные модули `(RSO, CalendarPlan, ...)` производит расчет и выдает важные статистические данные по проектам.

# Расширение

Чтобы выполнить код над документом необходимо:

* создать `.Net Standard 2.0+ или .Netframework 4.6.2+` библиотеку, реализовать интерфейс `ExtensionLib.Extension.IExtension` для какого-либо класса
* cобранную dll поместить в папку `~/ProgramData/Autodesk/Revit/Addins/2019/RevitSocketServer/extensions`
* создать контроллер в `Revit Service`
* создание запросов, **пример ниже:**

    ```C#
    var msg = new SocketServerEntities.Entity.Message
    {
        Body = new
        {
            // исполняемый код, состоит из двух частей "путь_до_длл исполняемый_метод"
            path = $"{Path.Combine("RSO", "RSOForm.dll")} RSO.RSOForWeb",   
            // авторизация         
            authorization,
            // нужные вам поля
            downloadurn,
            filePath,
            needToLoadLinks
        },
        // тип сообщения
        MsgType = MsgType.PushRequest,
        // тип запроса
        RequestType = RequestType.ExecuteRevitExtension,
        // id запроса
        Guid = key,
        // ключ сервиса, используйте IServiceKeyProvider.Next(), один ключ используется для одного сервиса
        ServiceKey = _serviceKey
    };
    ClientTcpObject.Request(msg); // отправка
    ClientTcpObject.OnReceive( // эвент при получении сообщения
        _serviceKey, 
        callback // delegate void CallBack(Message message)
    );
    ```

## **Важно:**

Сборки загружаются динамически - нет необходимости перезапускать ревит сервер

`Message.downloadurn` используется в создании очереди документов, очередь создается таким образом чтобы уменьшить кол-во ненужных скачиваний док-ов с `BIM360`, а также кол-во загрузок документов в `Revit`:

_Пример:_ поступило 5 запросов к следующим документам в следующем порядке: `[doc1 doc2 doc3 doc2 doc1 doc1]` преобразуется в `[doc1 doc1 doc1 doc2 doc2 doc3]`, хоть запрос к `doc3` был почти в начале в итоге он окажется в конце, пользователь будет дольше ждать, но суммарное время затраченное на обработку документов должно сильно уменьшить

Если Ваш метод контроллера должен быть доступен лишь авторизованному пользователю - используйте `[Authorize]`
