using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Represents a specific timestamp. Given in either julian or gregorian format. </summary>
/// <remarks> Gregorian is provided via <see cref="DateTime"/>. </remarks>
public struct time {
    #region VARIABLES
    /// <summary> The stored time in julian. </summary>
    public double julian {get; private set;}

    /// <summary> The stored time in julian century. </summary>
    public double julianCentury {get {return (this.julian - 2451545.0) / 36525.0;}}

    /// <summary> The stored time in gregorian. </summary>
    public DateTime date {get; private set;}
    #endregion

    #region CONSTRUCTORS
    /// <summary> Initialize a time with a julian time. </summary>
    public time(double julian) {
        this.julian = julian;
        this.date = julianToDate(julian);
    }

    /// <summary> Initialize a time via DateTime (gregorian). </summary>
    public time(DateTime date) {
        this.julian = dateToJulian(date);
        this.date = date;
    }
    #endregion

    #region PRIVATE METHODS
    private static double copySign(double n, double s) => Math.Sign(s) * n;
    #endregion

    #region INSTANCE METHODS
    /// <summary> Adds a julian value to the current time. </summary>
    public void addJulianTime(double value) {
        this.julian += value;
        this.date = julianToDate(this.julian);
    }

    /// <summary> Adds a TimeSpan (the difference between two DateTimes) to the current time. </summary>
    public void addDateTime(TimeSpan value) {
        this.date.Add(value);
        this.julian = dateToJulian(this.date);
    }
    #endregion

    #region STATIC METHODS
    /// <summary> Convert a DateTime to a julian time. </summary>
    public static double dateToJulian(DateTime date) {
        double Y = date.Year;
        double M = date.Month;
        double D = date.Day;
        double H = date.Hour;
        double Min = date.Minute;
        double S = date.Second;

        double JDN = 367 * Y - (int)((7 * (Y + (int)((M + 9) / 12.0))) / 4.0) + (int)((275 * M) / 9.0) + D + 1721013.5 +
          (H + Min / 60.0 + S / Math.Pow(60, 2)) / 24.0 - 0.5 * copySign(1, (100 * Y + M - 190002.5)) + 0.5;

        return JDN;
    }

    /// <summary> Convert a julian time to a DateTime. </summary>
    public static DateTime julianToDate(double julian) {
        // https://en.wikipedia.org/wiki/Julian_day#Julian_or_Gregorian_calendar_from_Julian_day_number
        double J = (double) ((int) julian);
        double f = J + 1401 + Math.Floor((Math.Floor((4.0 * J + 274277.0) / 146097) * 3.0) / 4.0) -38;
        double e = 4.0 * f + 3.0;
        double g = Math.Floor((e % 1461.0) / 4.0);
        double h = 5.0 * g + 2.0;

        double day = Math.Floor((h % 153.0) / 5.0) + 1;
        double month = ((Math.Floor(h / 153.0) + 2.0) % 12.0) + 1.0;
        double year = Math.Floor(e / 1461.0) - 4716.0 + Math.Floor((12.0 + 2.0 - month) / 12.0);

        double s = (julian - J) * 86400.0;
        double hours = Math.Floor(s / 3600.0);
        double minutes = Math.Floor((s - (hours * 3600.0)) / 60.0);
        double seconds = s - (hours * 3600.0 + minutes * 60.0);

        DateTime d = new DateTime((int) year, (int) month, (int) day, (int) hours, (int) minutes, (int) seconds);
        d = d.AddHours(12);

        return d;
    }

    /// <summary> Convert YYYY MMM DD HH:MM:SS.MMMM to time. </summary>
    public static double strDateToJulian(string date) {
        string[] splitDate = date.Split(new Char[] { ' ', ':'} , System.StringSplitOptions.RemoveEmptyEntries);

        double month = 0;
        if (splitDate[1] == "Jan") month = 1.0;
        if (splitDate[1] == "Feb") month = 2.0;
        if (splitDate[1] == "Mar") month = 3.0;
        if (splitDate[1] == "Apr") month = 4.0;
        if (splitDate[1] == "May") month = 5.0;
        if (splitDate[1] == "Jun") month = 6.0;
        if (splitDate[1] == "Jul") month = 7.0;
        if (splitDate[1] == "Aug") month = 8.0;
        if (splitDate[1] == "Sep") month = 9.0;
        if (splitDate[1] == "Oct") month = 10.0;
        if (splitDate[1] == "Nov") month = 11.0;
        if (splitDate[1] == "Dec") month = 12.0;


        double Y = Double.Parse(splitDate[2]);
        double M = month;
        double D = Double.Parse(splitDate[0]);
        double H = Double.Parse(splitDate[3]);
        double Min = Double.Parse(splitDate[4]);
        double S = Double.Parse(splitDate[5]);

        double JDN = 367 * Y - (int)((7 * (Y + (int)((M + 9) / 12.0))) / 4.0) + (int)((275 * M) / 9.0) + D + 1721013.5 +
          (H + Min / 60.0 + S / Math.Pow(60, 2)) / 24.0 - 0.5 * copySign(1, (100 * Y + M - 190002.5)) + 0.5;

        return JDN;
    }
    #endregion

    #region OVERRIDES/OPERATORS
    public override int GetHashCode() => (int) (this.julian * 1000);
    public override bool Equals(object obj)
    {
        if (!(obj is time)) return false;
        time t = (time) obj;
        return t.julian == this.julian;
    }
    public override string ToString() => $"{this.date.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'UTC'")} ({Math.Round(this.julian, 4)})";

    public static bool operator<(time t1, time t2) => t1.julian < t2.julian;
    public static bool operator>(time t1, time t2) => t1.julian > t2.julian;
    public static bool operator<=(time t1, time t2) => t1.julian <= t2.julian;
    public static bool operator>=(time t1, time t2) => t1.julian >= t2.julian;
    public static bool operator==(time t1, time t2) => t1.julian == t2.julian;
    public static bool operator!=(time t1, time t2) => t1.julian != t2.julian;
    #endregion
}
