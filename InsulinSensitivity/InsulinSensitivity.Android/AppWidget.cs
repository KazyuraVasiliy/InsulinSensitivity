using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content.Resources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace InsulinSensitivity.Droid
{
    [BroadcastReceiver(Label = "InsulinSensitivity Widget")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class AppWidget : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            var views = new RemoteViews(context.PackageName, Resource.Layout.Widget);

            // Начало обновления
            appWidgetManager.UpdateAppWidget(me, Process(views, true));

            // Установка текста
            SetTextViewText(views, context);

            // Регистрация обновления
            var updateIntent = new Intent(context, typeof(AppWidget));
            updateIntent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            updateIntent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            var updatePending = PendingIntent.GetBroadcast(context, 0, updateIntent, 0);
            views.SetOnClickPendingIntent(Resource.Id.widgetTextActiveInsulin, updatePending);

            // Регистрация настройки
            var settingsIntent = new Intent(context, typeof(AppWidget));
            settingsIntent.SetAction(SettingsClick);
            settingsIntent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            var settingsPending = PendingIntent.GetBroadcast(context, 0, settingsIntent, 0);
            views.SetOnClickPendingIntent(Resource.Id.widgetTextSettings, settingsPending);

            // Регистрация запуска приложения
            var startIntent = new Intent(context, typeof(MainActivity));
            var startPending = PendingIntent.GetActivity(context, 0, startIntent, 0);

            views.SetOnClickPendingIntent(Resource.Id.widgetLastEating, startPending);

            // Обновление
            appWidgetManager.UpdateAppWidget(me, views);

            // Завершение обновления
            appWidgetManager.UpdateAppWidget(me, Process(views, false));
        }

        /// <summary>
        /// Отображает / скрывает ProgressBar
        /// </summary>
        /// <param name="views"></param>
        /// <param name="isBegin"></param>
        /// <returns></returns>
        private RemoteViews Process(RemoteViews views, bool isBegin)
        {
            views.SetViewVisibility(Resource.Id.widgetProgress, isBegin ? ViewStates.Visible : ViewStates.Gone);
            views.SetViewVisibility(Resource.Id.widgetTextActiveInsulin, isBegin ? ViewStates.Gone : ViewStates.Visible);

            return views;
        }

        /// <summary>
        /// Идентификатор Action
        /// </summary>
        private static string SettingsClick = "SettingsTag";

        /// <summary>
        /// Текущий цвет
        /// </summary>
        private static Android.Graphics.Color Color = Android.Graphics.Color.White;

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Изменение цвета
            if (SettingsClick.Equals(intent.Action))
            {
                var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
                AppWidgetManager manager = AppWidgetManager.GetInstance(context);                

                Color = Color == Android.Graphics.Color.White
                    ? Android.Graphics.Color.Black
                    : Android.Graphics.Color.White;

                var fields = typeof(Resource.Id).GetFields()
                    .Where(x =>
                        x.Name.Contains("widgetText"));

                var views = new RemoteViews(context.PackageName, Resource.Layout.Widget);
                foreach (var field in fields)
                {
                    if (field.Name == "widgetTextLastEatingNutritionalIcon")
                        views.SetImageViewBitmap((int)field.GetValue(null), ConvertToImg("\xe04e", context, 40, Color));

                    else if (field.Name == "widgetTextLastEatingTimeIcon")
                        views.SetImageViewBitmap((int)field.GetValue(null), ConvertToImg("\xe120", context, 40, Color));

                    else if (field.Name == "widgetTextSettings")
                        views.SetImageViewBitmap((int)field.GetValue(null), ConvertToImg("\xe103", context, 60, Color));

                    else 
                        views.SetTextColor((int)field.GetValue(null), Color);                    
                }                    

                manager.UpdateAppWidget(me, views);
            }
        }

        private void SetTextViewText(RemoteViews views, Context context)
        {
            // Получение данных из БД
            var path = new AndroidDbPath().GetDatabasePath("InsulinSensitivityApp.db");
            DataAccessLayer.Contexts.Initialize.Init(path);

            using (var db = new DataAccessLayer.Contexts.ApplicationContext(path))
            {
                var user = db.Users
                    .Include(x => x.BolusType)
                    .Include(x => x.BasalType)
                    .FirstOrDefault();

                var eatings = user != null
                    ? db.Eatings
                        .Where(x =>
                            x.UserId == user.Id)
                        .Include(x => x.Exercise)
                            .ThenInclude(x => x.ExerciseType)
                        .Include(x => x.EatingType)
                        .Include(x => x.Injections)
                            .ThenInclude(x => x.BolusType)
                        .Include(x => x.IntermediateDimensions)
                        .Include(x => x.BasalType)
                        .Include(x => x.BolusType)
                        .ToList()
                        .OrderByDescending(x =>
                            x.DateCreated.Date)
                        .ThenByDescending(x =>
                            x.InjectionTime)
                        .ToList()
                    : null;

                var last = eatings
                    ?.FirstOrDefault();

                // Установка данных
                views.SetTextViewText(Resource.Id.widgetTextActiveInsulin, eatings != null && user != null
                    ? GlobalMethods.GetActiveInsulinForWidget(eatings, user).ToString()
                : "0");

                views.SetImageViewBitmap(Resource.Id.widgetTextLastEatingTimeIcon, ConvertToImg("\xe120", context, 40, Color.White));
                views.SetTextViewText(Resource.Id.widgetTextLastEatingType, last?.EatingType?.Name ?? "Нет приёмов пищи");
                views.SetTextViewText(Resource.Id.widgetTextLastEatingTime, last != null
                    ? $"{last?.InjectionTime.Hours:00}:{last?.InjectionTime.Minutes:00} ({last?.Pause} мин)"
                    : "");

                views.SetImageViewBitmap(Resource.Id.widgetTextLastEatingNutritionalIcon, ConvertToImg("\xe04e", context, 40, Color.White));
                views.SetTextViewText(Resource.Id.widgetTextLastEatingNutritional, last != null
                    ? $"{last?.Protein} / {last?.Fat} / {last?.Carbohydrate}"
                    : "");

                views.SetTextViewText(Resource.Id.widgetTextLastEatingWorkingTime, last != null
                    ? $"(до {last?.WorkingTime.Hours:00}:{last?.WorkingTime.Minutes:00})"
                    : "");

                views.SetImageViewBitmap(Resource.Id.widgetTextSettings, ConvertToImg("\xe103", context, 60, Color.White));
                views.SetViewVisibility(Resource.Id.widgetTextLastEatingWorkingTime, last?.GlucoseEnd == null 
                    ? ViewStates.Visible 
                    : ViewStates.Gone);
            } 
        }

        private Bitmap ConvertToImg(String text, Context context, int size, Color color)
        {
            Bitmap btmText = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb4444);
            Canvas cnvText = new Canvas(btmText);

            Typeface tf = ResourcesCompat.GetFont(context, Resource.Font.typicons);

            Paint paint = new Paint();
            paint.AntiAlias = true;
            paint.SubpixelText = true;
            paint.SetTypeface(tf);
            paint.SetARGB(color.A, color.R, color.G, color.B);
            paint.TextSize = size;

            cnvText.DrawText(text, 5, size - 10, paint);
            return btmText;
        }
    }
}