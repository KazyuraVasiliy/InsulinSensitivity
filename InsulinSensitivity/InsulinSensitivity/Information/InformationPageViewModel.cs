﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

using DataAccessLayer.Contexts;
using BusinessLogicLayer.ViewModel;
using BusinessLogicLayer.Service;
using Models = DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace InsulinSensitivity.Information
{
    public class InformationPageViewModel : ObservableBase
    {
        #region Properties

        public string Information { get; set; } =
            "Данное приложение создано в информативных целях и для упрощения расчетов дозировок инсулина людьми с сахарным диабетом 1 типа. " +
            "Все решения относительно постановки фактических доз пользователь принимает самостоятельно.\n\n" +
            "Предполагается, что фоновый базальный инсулин подобран корректно и ваша гликемия будет ровной при отсутствии приема пищи.\n\n" +
            "Предварительные пищевые коэффициенты рассчитываются в программе автоматически и указаны в информации о Пользователе.Расчет чувствительности к инсулину выполняется автоматически после завершения каждого приема пищи – в момент полной отработки еды и инсулина под эту еду.\n\n" +
            "Структура программы основана на периодах – условных промежутках времени, названия которых совпадают с типами приемов пищи.Пример: период завтрака начинается, когда вы ставите инъекцию болюса на еду, и заканчивается только тогда, когда ВСЯ еда в этом приеме пищи полностью переварится (включая перекусы, если они были). Скорость пищеварения у всех разная, но чаще всего это занимает около 5ч для сбалансированной по соотношению БЖУ пищи.Преимущественно углеводная еда переваривается за 3-4ч, а очень жирная и калорийная – за 6ч.\n\n" +
            "Создавая новый прием пищи для расчета дозы, вам потребуется: измерить сахар перед едой, самостоятельно рассчитать БЖУ всей пищи, обязательно указать планируемую активность и др.данные по вашему усмотрению.В первые 1-2 суток программа проанализирует вашу потребность в инсулине, а начиная с 3-их суток – начнет выдавать рекомендательные дозы.\n\n" +
            "За раз вы можете создать только 1 текущий прием пищи, который будет активен до ввода сахара на отработке. После ввода отработки прием пищи будет закрыт, и вы не сможете вернуться к нему и изменить информацию.\n\n" +
            "Для каждого завершенного приема пищи выполняется расчет точности предсказания ФЧИ (чувствительности к инсулину) и для программы, и для пользователя (в случае, если пользователь вводил свой предположительный ФЧИ при расчете).\n\n" +
            "Ожидаемая точность предсказаний программы – от 90%. Потребность в инсулине может быть переменчива, но программа всегда обучается и выполняет любые расчеты дозировок только на основе предыдущих приемов пищи и их результатов. При смене чувствительности к инсулину в зависимости от времени суток, нагрузок и дня цикла – программа учтет изменения ФЧИ и скорректирует дозировки на последующие приемы пищи.\n\n" +
            "Будьте внимательны и не ожидайте, что расчетный ФЧИ программы будет идеально совпадать с реальным фактическим. Помните – чем больше доза инсулина, тем выше погрешность расчета сахара на отработке.И конечно, предсказать можно только то, что можно предсказать заранее.";

        #endregion
    }
}