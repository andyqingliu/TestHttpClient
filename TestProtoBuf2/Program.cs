using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xsjm.Logic.Protocols;

namespace TestProtoBuf2
{
    class Program
    {
        static byte[] revBytes = null;

        static void Main(string[] args)
        {
            bool isRunning = true;

            while (isRunning)
            {
                string readStr = Console.ReadLine();
                if (readStr.Equals("exit"))
                {
                    isRunning = false;
                }
                else
                {
                    TestRequestLocal();
                }
            }

            //TestRWProtoBuf();

            //Console.ReadLine();
        }

        private static void TestRWProtoBuf()
        {
            C2S_GetStudentInfo student = BuildStudent();

            string path = "D:/Work/TestProtoBuf2/TestProtoBuf2/TestProto.bin";
            FileStream fs = File.Create(path);
            ProtoBuf.Serializer.Serialize(fs, student);
            fs.Close();

            FileStream fsr = File.OpenRead(path);
            C2S_GetStudentInfo rStudent = ProtoBuf.Serializer.Deserialize<C2S_GetStudentInfo>(fsr);
            Console.WriteLine("id:" + rStudent.id);
            Console.WriteLine("name:" + rStudent.name);
            Console.WriteLine("age:" + rStudent.age);
            for (int i = 0; i < rStudent.person.Count; i++)
            {
                Person p = rStudent.person[i];
                Console.WriteLine(string.Format("person_{0}:value_{1}", i + 1, p.value));
            }
        }

        private static C2S_GetStudentInfo BuildStudent()
        {
            C2S_GetStudentInfo student = new C2S_GetStudentInfo();
            student.id = 1000;
            student.name = "liuqing";
            student.age = 100;
            for (int i = 0; i < 1000; i++)
            {
                Person p = new Person();
                p.value = 10000 + i;

                student.person.Add(p);
            }
            return student;
        }

        private static void TestRequestLocal()
        {
            string searchStr = "C# is a language for developers!This is test for C# and Java!";
            Uri uri = new Uri("http://127.0.0.1:8888");
            string url = "http://127.0.0.1:8888";
            WebClient wc = new WebClient();
            wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            wc.Encoding = Encoding.UTF8;
            wc.UploadDataCompleted += new UploadDataCompletedEventHandler(OnUploadDataCompleted);
            //byte[] upBytes = wc.UploadData(url, "POST", Encoding.Default.GetBytes(searchStr));
            C2S_GetStudentInfo student = BuildStudent();
            MemoryStream s = new MemoryStream();
            ProtoBuf.Serializer.Serialize(s, student);
            
            try
            {
                byte[] buff = StreamToByteArray(s);
                Console.WriteLine("send len: " + buff.Length);

                wc.UploadDataAsync(uri, "POST", buff);
            }
            catch(WebException we)
            {
                Console.WriteLine("Exception:" + we.ToString());
            }
        }

        private static byte[] StreamToByteArray(MemoryStream s)
        {
            //if(null != s)
            //{
            //    byte[] bs = new byte[s.Length];
            //    s.Read(bs, 0, bs.Length);
            //    //s.Seek(0, SeekOrigin.Begin);
            //    s.Close();

            //    return bs;
            //}
            return s.ToArray();
            // return null;
        }

        private static MemoryStream BytesToStream(byte[] bytes)
        {
            if (null == bytes) return null;

            MemoryStream ms = new MemoryStream(bytes);
            return ms;
        }

        private static void OnUploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            Console.WriteLine("Upload data complete!");
            revBytes = e.Result;
            if (null != revBytes)
            {
                C2S_GetFriendList_message fm = ProtoBuf.Serializer.Deserialize<C2S_GetFriendList_message>(BytesToStream(revBytes));
                Console.WriteLine("ReceiveBytes,C2S_GetFriendList_message result:{0},person value:{1}", fm.result,fm.p.value );
            }
        }
    }
}
