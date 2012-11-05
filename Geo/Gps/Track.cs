using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;
using Geo.Gps.Metadata;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Gps
{
    public class Track : IRavenIndexable, IHasLength
    {
        public Track()
        {
            Metadata = new TrackMetadata();
            Segments = new List<TrackSegment>();
        }

        public TrackMetadata Metadata { get; private set; }
        public List<TrackSegment> Segments { get; set; }

        public LineString ToLineString()
        {
            return new LineString(Segments.SelectMany(x=>x.Fixes).Select(x => x.Coordinate));
        }

        public TrackSegment GetFirstSegment()
        {
            return Segments.Count == 0 ? default(TrackSegment) : Segments[0];
        }

        public TrackSegment GetLastSegment()
        {
            return Segments.Count == 0 ? default(TrackSegment) : Segments[Segments.Count - 1];
        }

        public IEnumerable<Fix> GetAllFixes()
        {
            return Segments.SelectMany(x => x.Fixes);
        }

        public Fix GetFirstFix()
        {
            var segment = GetFirstSegment();
            return segment == null ? default(Fix) : segment.GetFirstFix();
        }

        public Fix GetLastFix()
        {
            var segment = GetLastSegment();
            return segment == null ? default(Fix) : segment.GetLastFix();
        }

        public Speed GetAverageSpeed()
        {
            return new Speed(CalculateLength().SiValue, GetDuration());
        }

        public TimeSpan GetDuration()
        {
            return GetLastFix().TimeUtc - GetFirstFix().TimeUtc;
        }

        public Distance CalculateLength()
        {
            return Segments.SelectMany(x => x.Fixes).Select(x => x.Coordinate).CalculateShortestDistance();
        }

        public void Quantize(double seconds = 0)
        {
            foreach (var segment in Segments)
                segment.Quantize(seconds);
        }

        string IRavenIndexable.GetIndexString()
        {
            return ToLineString().ToWktString();
        }

        public Distance GetLength()
        {
            return GetAllFixes().CalculateShortestDistance();
        }
    }
}