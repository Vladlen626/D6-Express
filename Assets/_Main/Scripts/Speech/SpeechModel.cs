using System.Collections.Generic;

public class SpeechModel
{
	private readonly Dictionary<int, Speech> speechDict = new();

    public SpeechModel(IEnumerable<Speech> speeches)
    {
        foreach (var speech in speeches)
		{
			speechDict[speech.Id] = speech;
		}
    }

    public Speech GetSpeech(int id)
	{
		return speechDict.TryGetValue(id, out var speech) ? speech : null;
	}
}