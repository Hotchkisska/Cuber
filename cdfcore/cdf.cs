using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CDFcore;

namespace CDFcoreShell{	
    class Program{	
		
		static void Loop(){
			Console.WriteLine("CDF core v.1");
			while(true){
				Console.Write(">>> ");
				string com = Console.ReadLine();
				string[] words = com.Split(new char[] { ' ' });
				//Console.WriteLine(com);
				
				if(com == "help" || com == "h" || com == "?"){
					
				}else if(words[0] == "read"){
					if(words.Length == 2){
						string cdfPath = words[1];
						
						CDFfile obj = new CDFfile();
						obj.ReadFile(cdfPath);
						foreach(var Pair in obj.IntValues){
							Console.WriteLine(string.Format("INT  {0}  {1}", Pair.Key, Pair.Value));
						}
						
						foreach(var Pair in obj.StringValues){
							Console.WriteLine(string.Format("STR  {0}  {1}", Pair.Key, Pair.Value));
						}
						
						foreach(var Pair in obj.DoubleValues){
							Console.WriteLine(string.Format("DBL  {0}  {1}", Pair.Key, Pair.Value));
						}
					}
				}
			}
		}
		
		
        static void Main(string[] args){
			if(args.Length == 0){
				Loop();
			}
        }
    }
}