using System;
using System.Collections.Generic;

public class LocationHelper
{
    private const double EarthRadius = 6371; // Радиус Земли в километрах

    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Преобразуем градусы в радианы
        lat1 = ToRadians(lat1);
        lon1 = ToRadians(lon1);
        lat2 = ToRadians(lat2);
        lon2 = ToRadians(lon2);

        // Вычисляем разницу широт и долгот
        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        // Применяем формулу Хаверсина
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Возвращаем расстояние в километрах
        return EarthRadius * c;
    }

    public static SavedModel FindNearestModel(double latitude, double longitude, List<SavedModel> models)
    {
        SavedModel nearestElement = null;
        double minDistance = double.MaxValue;

        foreach (var m in models)
        {
            double distanceToElement = CalculateDistance(latitude, longitude, m.Latitude, m.Longitude);

            if (distanceToElement < minDistance)
            {
                minDistance = distanceToElement;
                nearestElement = m;
            }
        }

        return nearestElement;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}

[Serializable]
public class SavedModel
{
    public string SavedPath;
    public double Latitude;
    public double Longitude;
}