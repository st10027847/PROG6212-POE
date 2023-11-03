using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROG6212_CustomClassLib
{

    // Module Class
    public class Module
    {
        // Module Properties and Methods
        public string Code { get; set; }
        public string Name { get; set; }
        public int Credits { get; set; }
        public int ClassHoursPerWeek { get; set; }
        public int SelfStudyHours { get; set; }
        public int RecordedHours { get; set; }
        public int RemainingHoursThisWeek { get; set; }
        public List<StudyRecord> StudyRecords { get; set; } = new List<StudyRecord>();

        // Method for capturing the Recorded Hours For the Current Week
        public int GetRecordedHoursForWeek(DateTime startOfWeek, DateTime endOfWeek)
        {
            // Logic for capturing Recorded Hours for Current Week
            int recordedHours = 0;

            foreach (StudyRecord record in StudyRecords)
            {
                if (record.Date >= startOfWeek && record.Date <= endOfWeek)
                {
                    recordedHours += record.Hours;
                }
            }

            return recordedHours;
        }
    }

    // StudyRecord Class
    public class StudyRecord
    {
        // StudyRecord properties
        public DateTime Date { get; set; }
        public int Hours { get; set; }
    }
}