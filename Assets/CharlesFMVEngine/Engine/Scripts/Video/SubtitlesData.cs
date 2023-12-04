using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CharlesEngine
{
	public class SubtitleItem
	{
		public readonly int Index;
		public readonly string Text;
		public readonly float Start; //in seconds
		public readonly float End; //in seconds

		public SubtitleItem(int index, string text, float start, float end)
		{
			Index = index;
			Text = text;
			Start = start;
			End = end;
		}
	}

	public class SubtitlesData
	{
		private string _fileName;
		public readonly List<SubtitleItem> Items = new List<SubtitleItem>(); // of SubtitleItem
		private string _newLine;

		private int _lastTimeEndTime = -1000; // helper to warn against the same time being used twice
	
		public SubtitlesData(string text, string fileName)
		{
			_fileName = fileName;
			ParseSubRip(text);
		}
	
		private void ParseSubRip(string text)
		{
			bool winNl = text.IndexOf("\r\n") > 0;
			_newLine = winNl ? "\r\n" : "\n";
			string doubleLine = _newLine + _newLine;
			string tripleLine = doubleLine + _newLine;
		
			var rx = new Regex( doubleLine );		
			string[] itemTexts = rx.Split(text);
			if (text.IndexOf(tripleLine) > 0)
			{
				Debug.LogError("Subtitles parse failed - too many newlines ["+_fileName+"]");
				return;
			}
			string itemText;
			for (var index = 0; index < itemTexts.Length; index++)
			{
				itemText = itemTexts[index];
				if (index == itemTexts.Length - 1 && itemText == "")
					break; //last part
				CreateSubRipItem(itemText);
			}
		}

		private void CreateSubRipItem(string itemText)
		{
			string error = null;
			var rows = itemText.Split(new[] {_newLine}, StringSplitOptions.None );
			if (rows.Length < 3)
			{
				error = "insuficient row count";
			}
			else if (rows[0] == null || int.Parse(rows[0]) == 0)
			{
				error = "invalid index";
			}
			else if (rows[1] == null || rows[1].Length != 29 || rows[1].IndexOf(" --> ") < 0)
			{
				error = "invalid timestamps "+itemText;
			}
			if (error != null)
			{
				Debug.LogError("Subtitle parse error:" + error+" ["+_fileName+"]");
				return;
			}
		
			int index = int.Parse(rows[0]);
			string time = rows[1];
			var timestamps = time.Split(new[] {" --> "}, StringSplitOptions.None);
			var timeStart = TimestampToMiliseconds(timestamps[0]);

			if (timeStart - _lastTimeEndTime < 50)
			{
			//	Debug.LogWarning("There is not enough time between subtitles "+timestamps[0]+" ["+_fileName+"]");
				timeStart += 50;
			}
		
			var timeEnd = TimestampToMiliseconds(timestamps[1]);
			string subtitleText = rows[2];
			for (int i = 3; i < rows.Length; i++)
			{
				subtitleText += "\n" + rows[i];
			}
		
			var subtitleItem = new SubtitleItem(index, subtitleText, timeStart/1000f, timeEnd/1000f);
			Items.Add(subtitleItem);
		
			_lastTimeEndTime = timeEnd;
		}

		private int TimestampToMiliseconds(string timestamp)
		{
			var clock = timestamp.Split(':');
			var hours = int.Parse(clock[0]);
			var minutes = int.Parse(clock[1]);
			var secondsAr = clock[2].Split(',');
			var seconds = int.Parse(secondsAr[0]);
			var miliseconds = int.Parse(secondsAr[1]);
			miliseconds += (seconds * 1000) + (minutes * 60000) + (hours * 360000);
			return miliseconds;
		}

		public float EndSeconds()
		{
			return Items[Items.Count - 1].End;
		}

		public override string ToString()
		{
			var str = new StringBuilder();
			foreach (var item in Items)
			{
				str.AppendLine(item.Text);
			}

			return str.ToString();
		}
	}
}