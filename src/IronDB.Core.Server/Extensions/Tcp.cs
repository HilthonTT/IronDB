using System.Net.NetworkInformation;

namespace IronDB.Core.Server.Extensions;

public static class Tcp
{
    public static IPGlobalProperties? GetPGlobalPropertiesSafely()
    {
		try
		{
			return IPGlobalProperties.GetIPGlobalProperties();

        }
		catch (Exception)
		{
			return null;
		}
    }

	public static TcpConnectionInformation[] GetActiveTcpConnectionsSafely(this IPGlobalProperties? properties)
	{
		try
		{
			if (properties is null)
			{
				return [];
			}

			return properties.GetActiveTcpConnections();
		}
		catch (PlatformNotSupportedException)
		{
			return [];
		}
	}

    public static TcpStatistics? GetTcpIPv4StatisticsSafely(this IPGlobalProperties? properties)
	{
		try
		{
			if (properties is null)
			{
				return null;
			}

			return properties.GetTcpIPv4Statistics();
		}
		catch (PlatformNotSupportedException)
		{
			return null;
		}
	}

	public static TcpStatistics? GettcpIpv6StatisticsSafely(this IPGlobalProperties? properties)
	{
		try
		{
			if (properties is null)
			{
				return null;
			}

			return properties.GetTcpIPv6Statistics();
		}
		catch (PlatformNotSupportedException)
		{
			return null;
		}
	}

    public static long? GetConnectionsAcceptedSafely(this TcpStatistics? statistics)
	{
		try
		{
			if (statistics is null)
			{
				return null;
			}

			return statistics.ConnectionsAccepted;
		}
		catch (PlatformNotSupportedException)
		{
			return null;
		}
	}

    public static long? GetConnectionsInitiatedSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
			{
                return null;
            }

            return statistics.ConnectionsInitiated;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetCumulativeConnectionsSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.CumulativeConnections;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetCurrentConnectionsSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.CurrentConnections;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetErrorsReceivedSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.ErrorsReceived;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetFailedConnectionAttemptsSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.FailedConnectionAttempts;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetMaximumConnectionsSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.MaximumConnections;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetMaximumTransmissionTimeoutSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.MaximumTransmissionTimeout;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetMinimumTransmissionTimeoutSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.MinimumTransmissionTimeout;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetResetConnectionsSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.ResetConnections;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetResetsSentSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.ResetsSent;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetSegmentsReceivedSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.SegmentsReceived;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetSegmentsResentSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.SegmentsResent;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    public static long? GetSegmentsSentSafely(this TcpStatistics? statistics)
    {
        try
        {
            if (statistics is null)
            {
                return null;
            }

            return statistics.SegmentsSent;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }
}
