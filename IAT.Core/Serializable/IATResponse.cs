using IAT.Core.Enumerations;
using java.io;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Serializable
{

    class ItemResponse
    {
        [XmlElement("BlockNum", Form = XmlSchemaForm.Unqualified, DataType = "int")]
        public int BlockNumber { get; set; } = 0;

        [XmlElement("ItemNum", Form = XmlSchemaForm.Unqualified, DataType = "int")]
        public int ItemNumber { get; set; } = 0;

        [XmlElement("ResponseTime", Form = XmlSchemaForm.Unqualified, DataType = "long")]
        public long ResponseTime { get; set; } = 0;

        [XmlElement("Error", Form = XmlSchemaForm.Unqualified, DataType = "boolean")]
        public bool Error { get; set; } = false;

        [XmlElement("PresentationNum", Form = XmlSchemaForm.Unqualified, DataType = "int")]
        public int PresentationNumber { get; set; } = 0;
    }


    internal class IATResponse
    {
        [XmlArray]
        [XmlArrayItem("IATResponseSetElement")]
        public List<ItemResponse> Responses { get; set; }


        public double Score()
        {
            long LatencySum = 0;
            int nLT300 = 0;
            for (int ctr = 0; ctr < Responses.Count; ctr++)
            {
                LatencySum += IATResponse[ctr].ResponseTime;
                if (IATResponse[ctr].ResponseTime < 300)
                    nLT300++;
            }
            if (nLT300 * 10 >= _IATResults.NumDataElements)
            {
                _IATScore = double.NaN;
                return;
            }

            List<IIATItemResponse> Block3 = new List<IIATItemResponse>();
            List<IIATItemResponse> Block4 = new List<IIATItemResponse>();
            List<IIATItemResponse> Block6 = new List<IIATItemResponse>();
            List<IIATItemResponse> Block7 = new List<IIATItemResponse>();

            for (int ctr = 0; ctr < _IATResults.NumDataElements; ctr++)
            {
                if (IATResponse[ctr].ResponseTime > 10000)
                    continue;

                switch (IATResponse[ctr].BlockNumber)
                {
                    case 3:
                        Block3.Add(IATResponse[ctr]);
                        break;

                    case 4:
                        Block4.Add(IATResponse[ctr]);
                        break;

                    case 6:
                        Block6.Add(IATResponse[ctr]);
                        break;

                    case 7:
                        Block7.Add(IATResponse[ctr]);
                        break;
                }
            }

            List<IIATItemResponse> InclusiveSDList1 = new List<IIATItemResponse>();
            List<IIATItemResponse> InclusiveSDList2 = new List<IIATItemResponse>();

            InclusiveSDList1.AddRange(Block3);
            InclusiveSDList1.AddRange(Block6);
            double mean = InclusiveSDList1.Select(r => r.ResponseTime).Average();
            double sd3_6 = Math.Sqrt(InclusiveSDList1.Select(r => (double)r.ResponseTime).Aggregate<double, double>(0, (sd, rt) => sd + Math.Pow(rt - mean, 2)) / (double)(InclusiveSDList1.Count - 1));

            InclusiveSDList2.AddRange(Block4);
            InclusiveSDList2.AddRange(Block7);
            mean = InclusiveSDList2.Select(r => r.ResponseTime).Average();
            double sd4_7 = Math.Sqrt(InclusiveSDList2.Select(r => (double)r.ResponseTime).Aggregate<double, double>(0, (sd, rt) => sd + Math.Pow(rt - mean, 2)) / (double)(InclusiveSDList2.Count - 1));

            double mean3 = Block3.Select(r => r.ResponseTime).Average();
            double mean4 = Block4.Select(r => r.ResponseTime).Average();
            double mean6 = Block6.Select(r => r.ResponseTime).Average();
            double mean7 = Block7.Select(r => r.ResponseTime).Average();

            _IATScore = (((mean6 - mean3) / sd3_6) + ((mean7 - mean4) / sd4_7)) / 2;
        }

    }

}