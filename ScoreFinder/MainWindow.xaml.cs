using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace ScoreFinder
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	public partial class MainWindow : Window
	{
		// 100 tests max
		Button SelectedButton;
		string[][] Buttons = new string[100][];

		Button AddTest;
		int PannelSize = 35;
		int TestsCreated = 1;

		bool AutoChange = false; // Might be useless but ok

		private void CalculateTotalScore()
		{
			int Final = 0;

			// Failsafe
			if (TestsToTake == null || ScoreNeeded == null || OutOfFinal == null)
				return;

			if (TestsToTake.Text != "" && ScoreNeeded.Text != "")
			{
				int UnfinishedTests = Int32.Parse(TestsToTake.Text);
				int FinalPercent = Int32.Parse(ScoreNeeded.Text);

				if (UnfinishedTests != 0 && FinalPercent != 0)
				{
					int Count = 0;
					int TotalPercent = 0;

					for (int i = 0; i < Buttons.Length; i++)
					{
						if (Buttons[i] != null)
						{
							Count += 1;
							TotalPercent += Int32.Parse(Buttons[i][3]);
						}
					}

					if (Count != 0 && TotalPercent != 0)
					{
						// (a + b + x) / 3 = final
						Final = ((FinalPercent * (Count + UnfinishedTests)) - TotalPercent) / UnfinishedTests;

						Display.Text = Final.ToString() + "%";
					}
				}
			}

			// Out of score
			if (OutOfFinal.Text != "" && Final != 0)
			{
				float FinalOutOf = float.Parse(OutOfFinal.Text);
				int ScoreNeeded = (int)(((float)Final / (float)100) * FinalOutOf + 0.5); // Floats to int to basically floor the number (and to do math with decimals, along with 0.5 to always round up)

				FinalScoreDisplay.Text = ScoreNeeded.ToString();
			}
		}

		private int NextOpenSlot(string[][] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					return i;
				}
			}

			return -1;
		}

		private void UpdateData(Button button)
		{
			int ArrayPos = Int32.Parse(button.Name.Substring(6));
			string[] Data = Buttons[ArrayPos];

			if (SelectedButton != null)
			{
				SelectedButton.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDDDDDD"));
			}

			SelectedButton = button;

			SelectedButton.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFB4B4B4"));

			AutoChange = true;

			TestName.Text = Data[0];
			TestScore.Text = Data[1];
			TestOutOf.Text = Data[2];
			TestPercent.Text = Data[3];

			AutoChange = false;
		}

		private void TestClicked(object sender, RoutedEventArgs e)
		{
			Button ThisButton = sender as Button;
			UpdateData(ThisButton);
		}

		private void AddNewTest()
		{
			int NextPos = NextOpenSlot(Buttons);

			if (NextPos != -1)
			{
				// Create the data
				string[] Data = new string[4];

				Data[0] = "Test " + TestsCreated;
				Data[1] = "100";
				Data[2] = "100";
				Data[3] = "100";

				Buttons[NextPos] = Data;

				// Increase the pannel
				PannelSize += 30;
				TestPanel.Height = PannelSize;

				// Create the button
				TestPanel.Children.Remove(AddTest);
				Button NewTest = new Button();
				NewTest.Content = Data[0];
				NewTest.Width = 150;
				NewTest.Height = 30;
				NewTest.FontSize = 12;
				NewTest.Name = "Button" + NextPos.ToString(); // Using the name as a marker for the array index
				NewTest.Click += new RoutedEventHandler(TestClicked);
				TestPanel.Children.Add(NewTest);

				// Clean up the rest
				TestsCreated += 1;
				UpdateData(NewTest);
				NewAddTestButton();

				CalculateTotalScore();
			}
		}

		private void TestButton_Click(object sender, RoutedEventArgs e)
		{
			AddNewTest();
		}

		private void NewAddTestButton()
		{
			AddTest = new Button();
			AddTest.Content = "+";
			AddTest.Width = 150;
			AddTest.Height = 30;
			AddTest.Background = null;
			AddTest.BorderBrush = null;
			AddTest.Foreground = Brushes.White;
			AddTest.FontSize = 16;
			AddTest.Click += new RoutedEventHandler(TestButton_Click);
			TestPanel.Children.Add(AddTest);
		}

		private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
		private static bool IsTextAllowed(string text)
		{
			return !_regex.IsMatch(text);
		}

		public MainWindow()
		{
			InitializeComponent();

			NewAddTestButton();
			AddNewTest();
		}

		private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		}

		private void TestName_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SelectedButton != null && AutoChange == false)
			{
				TextBox NameBox = sender as TextBox;
				SelectedButton.Content = NameBox.Text;

				int ArrayPos = Int32.Parse(SelectedButton.Name.Substring(6));
				Buttons[ArrayPos][0] = NameBox.Text;
			}
		}

		private void TestScore_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SelectedButton != null && AutoChange == false)
			{
				AutoChange = true;

				TextBox ScoreBox = sender as TextBox;

				if (ScoreBox.Text != "")
				{

					int ArrayPos = Int32.Parse(SelectedButton.Name.Substring(6));
					string[] Data = Buttons[ArrayPos];

					Data[1] = ScoreBox.Text;
					Data[3] = ((int)(float.Parse(Data[1]) / (Int32.Parse(Data[2])) * 100)).ToString();

					TestPercent.Text = Data[3];

					CalculateTotalScore();
				}

				AutoChange = false;
			}
		}

		private void TestOutOf_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SelectedButton != null && AutoChange == false)
			{
				AutoChange = true;

				TextBox OutOfBox = sender as TextBox;

				if (OutOfBox.Text != "" && Int32.Parse(OutOfBox.Text) != 0)
				{

					int ArrayPos = Int32.Parse(SelectedButton.Name.Substring(6));
					string[] Data = Buttons[ArrayPos];

					Data[2] = OutOfBox.Text;
					Data[3] = ((int)(float.Parse(Data[1]) / (Int32.Parse(Data[2])) * 100)).ToString();

					TestPercent.Text = Data[3];

					CalculateTotalScore();
				}

				AutoChange = false;
			}
		}

		private void TestPercent_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SelectedButton != null && AutoChange == false)
			{
				AutoChange = true;

				TextBox PercentBox = sender as TextBox;

				if (PercentBox.Text != "")
				{
					int ArrayPos = Int32.Parse(SelectedButton.Name.Substring(6));
					Buttons[ArrayPos][3] = PercentBox.Text;

					Buttons[ArrayPos][1] = PercentBox.Text;
					Buttons[ArrayPos][2] = "100";

					TestScore.Text = PercentBox.Text;
					TestOutOf.Text = "100";


					CalculateTotalScore();
				}

				AutoChange = false;
			}
		}

		private void TestScore_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private void TestOutOf_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private void TestPercent_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private void DeleleteTest_Click(object sender, RoutedEventArgs e)
		{
			int ChildCount = TestPanel.Children.Count - 1; // -1 because of the + button
			Button LastButton = TestPanel.Children[ChildCount - 1] as Button;
			int ArrayPos = Int32.Parse(LastButton.Name.Substring(6));

			Buttons[ArrayPos] = null;
			TestPanel.Children.Remove(LastButton);

			// Set pannel size
			PannelSize -= 30;
			TestPanel.Height = PannelSize;

			// Handle children
			ChildCount = TestPanel.Children.Count - 1;
			if (ChildCount == 0)
			{
				AddNewTest();
				ChildCount = 1;
			}

			UpdateData(TestPanel.Children[ChildCount - 1] as Button);

			CalculateTotalScore();
		}

		private void TestsToTake_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private void ScoreNeeded_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private void TestsToTake_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox TestsToTakeBox = sender as TextBox;

			if (TestsToTakeBox.Text != "")
			{
				CalculateTotalScore();
			}
		}

		private void ScoreNeeded_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox ScoreNeededBox = sender as TextBox;

			if (ScoreNeededBox.Text != "")
			{
				CalculateTotalScore();
			}
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Minimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}
	}
}
