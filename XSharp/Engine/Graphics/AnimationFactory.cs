using System;

using XSharp.Engine.Entities;
using XSharp.Factories;
using XSharp.Math;
using XSharp.Math.Geometry;
using XSharp.Serialization;

namespace XSharp.Engine.Graphics;

public class AnimationFactory(Sprite sprite) : IndexedNamedFactoryList<Animation>
{
    private EntityReference<Sprite> sprite = sprite;

    public Sprite Sprite => sprite;

    public AnimationFactory() : this(null)
    {
    }

    public Animation Create(string frameSequenceName, Vector offset, int repeatX, int repeatY, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
    {
        return Create(frameSequenceName, offset, FixedSingle.ZERO, repeatX, repeatY, initialFrame, startVisible, startOn, mirrored, flipped);
    }

    public Animation Create(string frameSequenceName, Vector offset, FixedSingle rotation, int repeatX, int repeatY, int initialFrame = 0, bool startVisible = true, bool startOn = true, bool mirrored = false, bool flipped = false)
    {
        var animation = Create<Animation, AnimationReference>();
        animation.Initialize(frameSequenceName, offset, rotation, repeatX, repeatY, initialFrame, startVisible, startOn, mirrored, flipped);
        return animation;
    }

    new public AnimationReference GetReferenceTo(Animation animation)
    {
        return (AnimationReference) GetItemReferenceByIndex(animation.Index);
    }

    protected override DuplicateItemNameException<Animation> CreateDuplicateNameException(string name)
    {
        return new DuplicateAnimationNameException(name);
    }

    protected override void SetItemFactory(Animation animation, IndexedNamedFactoryItemReference<Animation> reference)
    {
        animation.sprite = sprite;
        ((AnimationReference) reference).sprite = sprite;
    }

    protected override void SetItemIndex(Animation animation, IndexedNamedFactoryItemReference<Animation> reference, int index)
    {
        animation.Index = index;
        ((AnimationReference) reference).TargetIndex = index;
    }

    protected override void SetItemName(Animation animation, IndexedNamedFactoryItemReference<Animation> reference, string name)
    {
        animation.name = name;
        ((AnimationReference) reference).TargetName = name;
    }

    internal void UpdateAnimationName(Animation animation, string name)
    {
        UpdateItemName(animation, name);
    }

    protected override Type GetDefaultItemReferenceType(Type itemType)
    {
        return typeof(AnimationReference);
    }

    protected override void SetReferenceTo(int index, IndexedNamedFactoryItemReference<Animation> reference)
    {
        base.SetReferenceTo(index, reference);

        ((AnimationReference) reference).sprite = sprite;
    }

    protected override void SetReferenceTo(string name, IndexedNamedFactoryItemReference<Animation> reference)
    {
        base.SetReferenceTo(name, reference);

        ((AnimationReference) reference).sprite = sprite;
    }

    protected override void OnCreateReference(IndexedNamedFactoryItemReference<Animation> reference)
    {
        ((AnimationReference) reference).sprite = sprite;
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
}