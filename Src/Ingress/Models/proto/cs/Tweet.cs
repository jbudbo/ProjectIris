using System.Linq;

namespace Iris.Proto;

public sealed partial class Tweet
{
	public Tweet(Ingress.Models.Tweet dto)
	{
		Id = dto.id;
		Text = dto.text;
		Entities = new Entity(dto.entities);
	}
}

public sealed partial class Entity
{
	public Entity(Ingress.Models.TweetEntity dto)
	{
		if (dto.annotations?.Any() ?? false)
			Annotations.AddRange(dto.annotations.Select(static a => new AnnotationEntity(a)));

		if (dto.hashtags?.Any() ?? false)
			Hashtags.AddRange(dto.hashtags.Select(static ht => new HashTagEntity(ht)));
	
		if (dto.mentions?.Any() ?? false)
			Mentions.AddRange(dto.mentions.Select(static m => new MentionEntity(m)));
	
		if (dto.urls?.Any() ?? false)
			Urls.AddRange(dto.urls.Select(static u => new UrlEntity(u)));
	}
}

public sealed partial class AnnotationEntity
{
	public AnnotationEntity(Ingress.Models.TweetAnnotation dto)
	{
		Start = dto.start;
		End = dto.end;
		NormalizedText = dto.normalized_text;
		Probability = dto.probability;
		Type = dto.type;
	}
}

public sealed partial class HashTagEntity
{
	public HashTagEntity(Ingress.Models.TweetHashtag dto)
	{
		Start = dto.start;
		End = dto.end;
		Tag = dto.tag;
	}
}

public sealed partial class MentionEntity
{
	public MentionEntity(Ingress.Models.TweetMention dto)
	{
		Start = dto.start;
		End = dto.end;
		Id = dto.id;
		Username = dto.username;
	}
}

public sealed partial class UrlEntity
{
	public UrlEntity(Ingress.Models.TweetUrl dto)
	{
		Start = dto.start;
		End = dto.end;
		DisplayUrl = dto.display_url;
		ExpandedUrl = dto.expanded_url;
		Url = dto.url;
	}
}