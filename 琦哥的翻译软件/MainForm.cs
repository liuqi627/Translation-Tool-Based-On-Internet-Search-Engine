using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace 琦哥的翻译软件
{
    public partial class MainForm : Form
    {
        private static int count = 0;
        public static string final_answer = "";//最后呈现的回答
        public static double final_similarity = 0;//最后的相似度
        public MainForm()
        {
            InitializeComponent();
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            final_answer = "";//初始化答案
            final_similarity = 0;//初始化相似度
            int num = 5; //搜索条数
            string url = "http://www.google.com.hk/search?hl=zh-CN&source=hp&q=" + "site:zhidao.baidu.com " + 
                textBox1.Text.Trim() + " 的英语是什么" + "&aq=f&aqi=&aql=&oq=&num=" + num + "";
            string html = search(url, "utf-8");
            if (!string.IsNullOrEmpty(html))
            {
                googleSearch google = new googleSearch();
                List<Keyword> keywords = google.GetKeywords(html, textBox1.Text.Trim());
                dataGridView1.DataSource = keywords;
                List<Candidate> candidates = new List<Candidate> { };
                foreach (Keyword k in keywords)
                {
                    //开始获得每个条目的html
                    Encoding gb2312 = Encoding.GetEncoding("GB2312");
                    string uri = k.Link;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    string htmlString = string.Empty;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Stream stream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(stream, Encoding.Default);
                        htmlString = reader.ReadToEnd();
                        stream.Close();
                        reader.Close();
                        stream.Close();
                    }
                    //获取到了这个页面，开始从这个页面里找答案
                    Regex reg = new Regex("class=\"best-text\\smb-10\">(?<wanting>[a-zA-Z\\s,.]*)", RegexOptions.IgnoreCase);
                    MatchCollection mcTable = reg.Matches(htmlString);
                    foreach (Match mTable in mcTable)
                    {
                        if (mTable.Success)
                        {
                            Candidate candidate = new Candidate();
                            candidate.Chinese = k.Title.Substring(0, k.Title.Length - 5);
                            candidate.English = mTable.Groups["wanting"].Value;
                            candidate.Number = k.ID;
                            candidates.Add(candidate);
                        }
                    }
                }//检索每个条目完毕
                dataGridView2.DataSource = candidates;
                //下面的工作是判断是否候选结果集中是否都为空，为空则调用百度翻译
        
                bool have_candidate = false;
                foreach (Candidate c in candidates)
                {
                    if (!c.English.Equals(""))
                    {
                        have_candidate = true;//出现了英文翻译
                        break;
                    }
                }
                if (have_candidate)//有的话就要判断相似度了
                {
                    
                    foreach (Candidate c in candidates)
                    {
                        count = 0;
                        getLCSLength(textBox1.Text.Trim(), c.Chinese);
                        c.Similarity = 2.0 * count / (textBox1.Text.Trim().Length + c.Chinese.Length);
                        if (c.Similarity >= final_similarity)
                        {
                            final_answer = c.English;
                            final_similarity = c.Similarity;
                        }
                    }
                }
                else
                {
                    BaiduTranslate(textBox1.Text.Trim());
                }
            }//有搜索结果的话完毕，后面应该有没有搜索结果的情况，即调用百度翻译
            else
            {
                BaiduTranslate(textBox1.Text.Trim());
            }
            textBox2.Text = final_answer;
            if (final_similarity > 0.7)
            {
                pictureBox2.Visible = false;
                pictureBox3.Visible = false;
                pictureBox1.Show();
               
            }
            else if (final_similarity <= 0.7 && final_similarity >= 0.5)
            {   
                pictureBox1.Visible = false;
                pictureBox3.Visible = false;
                pictureBox2.Show();
                
            }
            else
            {
                pictureBox1.Visible = false;
                pictureBox2.Visible = false;
                pictureBox3.Show();

            }
            
        }

        /// <summary>
        /// 相似度判断
        /// </summary>
        /// <param name="origin">原始中文句</param>
        /// <param name="vs_origin">调用百度翻译</param>

        public static void getLCSLength(String str1, String str2)
	    {
		    char[] x = str1.ToCharArray();
		    char[] y = str2.ToCharArray();
		
		    int[,] c = new int[x.Length+1,y.Length+1];
		
		    for(int i=1; i<x.Length+1; ++i)
			    for(int j=1; j<y.Length+1; ++j)
			    {
				    if(x[i-1] == y[j-1])
					    c[i,j] = c[i-1,j-1]+1;
				    else if(c[i-1,j]>=c[i,j-1])
					    c[i,j] = c[i-1,j];
				    else
					    c[i,j] = c[i,j-1];					
			    }		
		    printLCS(c, x, y,  x.Length, y.Length);
	    }

        /// <summary>
        /// 计算LCS
        /// </summary>

        public static void printLCS(int[,] c, char[] x, char[] y, int i, int j)
        {
            //int count = 0;
            if (i == 0 || j == 0)
                return;
            if (x[i - 1] == y[j - 1])
            {
                printLCS(c, x, y, i - 1, j - 1);
                Console.WriteLine(x[i - 1]);
                count++;
            }
            else if (c[i - 1, j] >= c[i, j - 1])
                printLCS(c, x, y, i - 1, j);
            else
                printLCS(c, x, y, i, j - 1);
        }
        
        /// <summary>
        /// 交给百度了
        /// </summary>
        /// <param name="origin">调用百度翻译</param>

        public void BaiduTranslate(string origin)
        {
            final_answer = "Sorry, We can't find it";
        }

            /// <summary>
        /// 搜索处理
        /// </summary>
        /// <param name="url">搜索网址</param>
        /// <param name="Chareset">编码</param>
        public string search(string url, string Chareset)
        {
            HttpState result = new HttpState();
            Uri uri = new Uri(url);
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            myHttpWebRequest.UseDefaultCredentials = true;
            myHttpWebRequest.ContentType = "text/html";
            myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.0; .NET CLR 1.1.4322; .NET CLR 2.0.50215;)";
            myHttpWebRequest.Method = "GET";
            myHttpWebRequest.CookieContainer = new CookieContainer();

            try
            {
                HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
                // 从 ResponseStream 中读取HTML源码并格式化 
                result.Html = readResponseStream(response, Chareset);
                result.CookieContainer = myHttpWebRequest.CookieContainer;
                return result.Html;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public string readResponseStream(HttpWebResponse response, string Chareset)
        {
            string result = "";
            using (StreamReader responseReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(Chareset)))
            {
                result = formatHTML(responseReader.ReadToEnd());
            }

            return result;
        }
        /// <summary>
        /// 描述:格式化网页源码
        /// 
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public string formatHTML(string htmlContent)
        {
            string result = "";

            result = htmlContent.Replace("&raquo;", "").Replace("&nbsp;", "")
                    .Replace("&copy;", "").Replace("/r", "").Replace("/t", "")
                    .Replace("/n", "").Replace("&amp;", "&");
            return result;
        }
    }
}
