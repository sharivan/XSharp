using System.Collections.Generic;

using XSharp.Engine.Entities;
using XSharp.Factories;
using XSharp.Serialization;

namespace XSharp.Engine.Graphics;

public class AnimationReference : IndexedNamedFactoryItemReference<Animation>
{
    internal EntityReference<Sprite> sprite;

    public Sprite Sprite => sprite;

    public AnimationReference()
    {
    }

    public override void Deserialize(ISerializer serializer)
    {
        sprite = serializer.ReadItemReference<EntityReference<Sprite>>(false);
        base.Deserialize(serializer);
    }

    public override void Serialize(ISerializer serializer)
    {
        serializer.WriteItemReference(sprite, false);
        base.Serialize(serializer);
    }

    protected override IndexedNamedFactory<Animation> GetFactory()
    {
        return Sprite?.Animations;
    }

    public static implicit operator Animation(AnimationReference reference)
    {
        return reference?.Target;
    }

    public static implicit operator AnimationReference(Animation animation)
    {
        return animation?.Sprite.Animations.GetReferenceTo(animation);
    }

    public static bool operator ==(AnimationReference reference1, AnimationReference reference2)
    {
        return ReferenceEquals(reference1, reference2)
            || reference1 is not null && reference2 is not null && reference1.Equals(reference2);
    }

    public static bool operator ==(AnimationReference reference, Animation animation)
    {
        return reference is null ? animation is null : EqualityComparer<Animation>.Default.Equals(reference.Target, animation);
    }

    public static bool operator ==(Animation animation, AnimationReference reference)
    {
        return reference == animation;
    }

    public static bool operator !=(AnimationReference reference1, AnimationReference reference2)
    {
        return !(reference1 == reference2);
    }

    public static bool operator !=(AnimationReference reference, Animation animation)
    {
        return !(reference == animation);
    }

    public static bool operator !=(Animation animation, AnimationReference reference)
    {
        return !(animation == reference);
    }

    public override bool Equals(object? obj)
    {
        var target = Target;
        return ReferenceEquals(obj, target)
            || obj != null
            && (
            obj is AnimationReference reference && EqualityComparer<Animation>.Default.Equals(target, reference.Target)
            || obj is Animation animation && EqualityComparer<Animation>.Default.Equals(target, animation)
            );
    }

    public override int GetHashCode()
    {
        return TargetIndex;
    }
}