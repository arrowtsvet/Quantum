using System;
using System.IO;

namespace PauliBasis
{
	class Program
	{
		static void Main(string[] arg)
		{
			int n = 3; 
			int N = (int)Math.Pow(2.0, (double)n);			
			ComplexMatrix A = new ComplexMatrix(N);				
			Bases01andPauli bases = new Bases01andPauli(n, N);

			A.readFromFile("input.txt", n);
			bases.ToPauli(A);
			bases.display();
			
		}
	}
	
	class ComplexMatrix{
		public double[,] a, b;

		public ComplexMatrix(int N){
			this.a  = new double[N,N];
			this.b  = new double[N,N];
		}
		public void readFromFile(string filename, int n){
			double[] numbers = new double[n * n * 2];
			string[] finput = File.ReadAllLines(filename);
			int k = 0;
			for (int i = 0; i < finput.Length; i++){
				string line = finput[i];
				if (finput[i] != "-"){
					numbers[k] = Double.Parse(line.Split(' ')[0]);
					numbers[k + 1] = Double.Parse(line.Split(' ')[1]);
					k += 2;
				}
			}
			for (int i = 0; i < n; i++){
				for (int j = 0; j < n; j++){
					this.a[i,j] = numbers[i * n + j];
					this.b[i,j] = numbers[(i + n) * n + j];
				}
			}
		}
	}
	
	class Bases01andPauli
	{
		public int n, N; // Объявляем число кубитов n и N=2^n

		public double[,] s, t;
		public string[,] sigma;
		
		public Bases01andPauli(int n, int N){
			this.n = n; this.N = N;
			this.s  = new double[N,N];
			this.t  = new double[N,N];
			this.sigma = new string[N,N];
		} 

		public void display(){
			Console.WriteLine("Матрица s[k,l]:\n");
				for (int k = 0; k < N; k++) 
				{ 
					Console.Write(" k = " + k + " ");
					for (int l = 0; l < N; l++) 
					{ Console.Write(" {0, 6:f2}", this.s[k,l]); }
					Console.WriteLine();
				}
				Console.WriteLine("\n");
				
				Console.WriteLine("Матрица t[k,l]:\n");
				for (int k = 0; k < N; k++) 
				{ 
					Console.Write(" k = " + k + "  ");
					for (int l = 0; l < N; l++) 
					{ Console.Write(" {0, 6:f2}", this.t[k,l]); }
					Console.WriteLine();
				}
				Console.WriteLine("\n");
				
				Console.WriteLine("Матрица sigma[k,l]:\n");
				for (int k = 0; k < N; k++) 
				{ 
					Console.Write(" k = " + k + "  ");
					for (int l = 0; l < N; l++) 
					{ Console.Write(" "+this.sigma[k,l]+" "); }
					Console.WriteLine();
				}
		}
		
		/*  
			Передаем в метод коэффициенты а[i,j]+I*b[i,j] в 
			стандартном базисе |i><j|. Метод возвращает 
			коэффициенты s[k,l]+I*t[k,l] в базисе Паули, 
			а также строки sigma[k,l]с номерами матриц Паули 
			в базисных элементах, соответствующих парам k,l.
		*/
		
		public void ToPauli(ComplexMatrix A)
		{
			this.s  = new double[N,N]; 
			this.t  = new double[N,N];
			this.sigma = new string[N,N];		
			
			for(int i=0; i<N; i++)  
			{
			for(int j=0; j<N; j++)
			{
			/*  В двойном цикле по i,j вычисляем номер строки
			    k=i^j в матрицах s[k,l] и t[k,l] поразрядной
			    операцией XOR (сложение по модулю 2)
			*/
				
				int k = i^j; 
				
			/*  I,J,K -- бинарные представления для i,j,k.
				ПРИМЕР: если  I = 0011, J = 0101 (n=4), то
				|I><J| = (1/N)*{(S0+S3)(S1+I*S2)(S1-I*S2)(S0-S3)},
				где Su -- операторы Паули (u = 0,1,2,3). 
				
				Массивы sgn[p],count[p] поразрядно (справа налево)
				указывают, соответственно, на знак второго 
				слагаемого и на наличие у него множителя I*. 
				Соответствие |I_k><J_k| --> sgn[p],count[p]:
				|0><0|~1,0; |0><1|~1,1; |1><0|~-1,1; |1><1|~-1,0;
				
				В ПРИМЕРе K = 0110 -- это номера операторов Паули
				S0, S1, S1, S0 -- первых слагаемых. Значения
				массивов: sgn[0]=-1, sgn[1]=-1, sgn[2]=1, sgn[3]=1,
				count[0]=0, count[1]=1, count[2]=1, count[0]=0.
			*/			
				int[] sgn = new int[n];
				int[] count = new int[n];
				string I, J, K;					 
				I = Convert.ToString(i, 2); 
				J = Convert.ToString(j, 2);
				K = Convert.ToString(k, 2);
				
			//  Переводим строки I,J,K в десятичные числа 
			//  I10,J10,K10. Например, 0011 --> 11, 1001 --> 1001.
			
				int I10, J10, K10;
				I10 = Convert.ToInt32(I, 10); 
				J10 = Convert.ToInt32(J, 10);
				K10 = Convert.ToInt32(K, 10);			
				
			//  Для 0 =< p < n формируем массивы sgn[p] и count[p]
			//  посредством выделения остатков (поразрядно) при
			//  делении ЦЕЛЫХ ДЕСЯТИЧНЫХ чисел I10, J10 на 10;
			//	частное от деления -- ЦЕЛОЕ ДЕСЯТИЧНОЕ!
				for(int p = 0; p < n; p++)
				{
					int Ir = I10%10; int Jr = J10%10;
					if(Ir == 0 && Jr == 0) 
						{ sgn[p] = 1; count[p] = 0; }
					else
					{	if(Ir == 0 && Jr == 1) 
							{ sgn[p] = 1; count[p] = 1; }
						else
						{	if(Ir == 1 && Jr == 0)
								{ sgn[p] = -1; count[p] = 1; }
							else { sgn[p] = -1; count[p] = 0; }
						}
					}					
					I10 /= 10; J10 /= 10; 
				}
			
			//  В нулевом (l=0) элементе строк с номером k:
				s[k,0] += A.a[i,j]; t[k,0] += A.b[i,j]; 
				sigma[k,0] = K;
				
			//	Добавляем нули слева, если длина строки K меньше n.
				for(int ind =0; ind < n-K.Length; ind++)
					{sigma[k,0] = '0'+sigma[k,0];}
			
			//  В цикле 1 =< l < N найдем остальные элементы строк
			//	с номером k в матрицах s[k,l], t[k,l], sigma[k,l].				
				for(int l=1; l < N; l++)
				{
					string L = Convert.ToString(l, 2);
					int L10 = Convert.ToInt32(L, 10);
				
				/*	Для данного l в цикле найдем: sign -- это
					произведение элементов sgn[p] с номерами p, 
					для которых в p-ом разряде сироки L стоит 1;
					counter -- это сумма элементов count[p]
					с тем же условием. Цикл -- последовательное 
					делениие ЦЕЛОГО ДЕСЯТИЧНОГО quotient = L10 
					(вида 10110) на 10, пока частное > 0.
				*/
					int sign = 1, counter = 0;
					int p = 0; int quotient = L10;
					while(quotient > 0)
					{
						if(quotient%10 == 1) 
						{ sign *= sgn[p]; counter += count[p]; }
						quotient /= 10;
						p++;
					}
				
				/*	Раскрытие всех скобок вида (Su+Sv), (Su+I*Sv),
					(Su-I*Sv), (Su-Sv) в (a[i,j]+I*b[i,j])*I><J|,
					дает коэффициенты sign * I^counter * a[i,j] 
					и I * sign * I^counter * b[i,j].
					
					Если counter = 4^q (q -- целое), то
					в s[k,l] запишем sign * a[i,j],
					в t[k,l] запишем sign * b[i,j].
					
					Если counter = 4^q+1, то
					в s[k,l] запишем -sign * b[i,j],
					в t[k,l] запишем  sign * a[i,j].
					
					Если counter = 4^q+2, то
					в s[k,l] запишем -sign * a[i,j],
					в t[k,l] запишем -sign * b[i,j].
					
					Если counter = 4^q+3, то
					в s[k,l] запишем  sign * b[i,j],
					в t[k,l] запишем -sign * a[i,j].
				*/
					if(counter%4 == 0) 
						{ s[k,l] += sign*A.a[i,j]; 
							t[k,l] += sign*A.b[i,j]; }
					else
					{
						if(counter%4 == 1) 
							{ s[k,l] -= sign*A.b[i,j]; 
								t[k,l] += sign*A.a[i,j]; }
						else
						{
							if(counter%4 == 2) 
								{ s[k,l] -= sign*A.a[i,j]; 
									t[k,l] -= sign*A.b[i,j]; }
							else 
								{ s[k,l] += sign*A.b[i,j]; 
									t[k,l] -= sign*A.a[i,j]; }
						}
					}
					
				/*	Для данных k,l заполняем матрицу sigma[k,l].
				
					(1) sigma[k,0] = K - это строка вида 010110;
						остальные элементы sigma[k,l] строки k 
						получаются из K заменами 0 на 3 и/или
						1 на 2 в некоторых (или всех) разрядах. 
						
					(2) Вычисляем (~l)&k, l&k, l&~k и переводим 
						их в бинарные строки NOTLK, LK, LNOTK,
						индексирующие значением 1 разряды,
						которые, соответстенно, не изменяются,
						заменяются на 2, заменяются на 3.
						
					(3)	Строки NOTLK, LK, LNOTK переводим в
						ДЕСЯТИЧНЫЕ числа NOTLK10, LK10, LNOTK10 
						с теми же цифрами 0 и 1 в разрядах 
						(например, 001101 --> 1101).
				*/	 
					int NOTlk = (~l)&k; 
					int lk = l&k;
					int lNOTk = l&~k;				
				
				/*	ВАЖНО! Если, например, l = 10, k = 110, то
					~l имеет бинарную строку 1111...11111101,
					а k -- строку 0000...00000110, поэтому в
					операции ~l&k (= 2) получим для этого числа
					бинарную строку 0000...00000010. Единицы
					слева в ~l не имеют значения. Но если взять
					операцию ~l|k, то полученный результат может 
					не соответствовать ожидаемому.
				*/				
					string NOTLK, LK, LNOTK;
					int NOTLK10, LK10, LNOTK10;
		
					NOTLK = Convert.ToString(NOTlk, 2);
					LK = Convert.ToString(lk, 2);
					LNOTK = Convert.ToString(lNOTk, 2);
					
					NOTLK10 = Convert.ToInt32(NOTLK, 10); 
					LK10 = Convert.ToInt32(LK, 10);
					LNOTK10 = Convert.ToInt32(LNOTK, 10);
					
					int skl = NOTLK10 + 2*LK10 + 3*LNOTK10; 
					string SKL = skl.ToString();
					
				//	Добавляем нули слева, если SKL.Length < n
					for(int ind =0; ind < n - SKL.Length; ind++)
					{SKL = '0'+SKL;}					
					
				//	Получим строку m_{n-1}...m_{0} из цифр 0,1,2,3
					sigma[k,l] = SKL;					
				}
			}
			}						
		}
	}
}	
