using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public abstract class ActivatedSensor : Sensor
{
    public interface Filter
    {
        bool EntityMatches(EntityComponent entityComponent);
    }

    public class EntityFilter : Filter
    {
        public EntityReference entityRef;

        public EntityFilter() { } // deserialization

        public EntityFilter(Entity e)
        {
            entityRef = new EntityReference(e);
        }

        public bool EntityMatches(EntityComponent entityComponent)
        {
            if (entityComponent == null)
                return false;
            return entityComponent.entity == entityRef.entity; // also matches clones
        }

        public override string ToString()
        {
            Entity e = entityRef.entity;
            if (e == null)
                return GUIPanel.StringSet.EntityRefNone;
            return e.ToString();
        }
    }

    // matches type of Entity or any of its Behaviors
    public class EntityTypeFilter : Filter
    {
        // :(
        public PropertiesObjectType _entityType; // will be serialized
        [XmlIgnore]
        public PropertiesObjectType entityType
        {
            get
            {
                if (_entityType != null && _entityType.type == null)
                {
                    // was deserialized. See comment on PropertiesObjectType for more info
                    PropertiesObjectType instance = GameScripts.FindTypeWithName(
                        GameScripts.entityFilterTypes, _entityType.fullName);
                    if (instance != null)
                    {
                        _entityType = instance;
                        return _entityType;
                    }
                    instance = GameScripts.FindTypeWithName(
                            GameScripts.behaviors, _entityType.fullName);
                    if (instance != null)
                    {
                        // BehaviorType can't be serialized
                        _entityType = new PropertiesObjectType(instance, null);
                        return _entityType;
                    }
                    Debug.Log("Couldn't find matching filter type for " + _entityType.fullName + "!");
                    _entityType = Entity.objectType;
                }
                return _entityType;
            }
            set
            {
                if (value is BehaviorType)
                    // BehaviorType can't be serialized
                    _entityType = new PropertiesObjectType(value, null);
                else
                    _entityType = value;
            }
        }

        public EntityTypeFilter() { } // deserialization

        public EntityTypeFilter(PropertiesObjectType type)
        {
            entityType = type;
        }

        public bool EntityMatches(EntityComponent entityComponent)
        {
            if (entityComponent == null)
                return false;
            if (entityType.type.IsInstanceOfType(entityComponent.entity))
                return true;
            bool isOn = entityComponent.IsOn();
            foreach (EntityBehavior behavior in entityComponent.entity.behaviors)
            {
                if (isOn && behavior.condition == EntityBehavior.Condition.OFF)
                    continue; // not active
                if (!isOn && behavior.condition == EntityBehavior.Condition.ON)
                    continue; // not active
                if (entityType.type.IsInstanceOfType(behavior))
                    return true;
            }
            return false;
        }

        public override string ToString() => entityType.fullName;
    }

    // for maps before version 10
    public class TagFilter : Filter
    {
        public byte tag;

        public TagFilter() { } // deserialization

        public TagFilter(byte tag)
        {
            this.tag = tag;
        }

        public bool EntityMatches(EntityComponent entityComponent)
        {
            if (entityComponent == null)
                return false;
            return entityComponent.entity.tag == tag;
        }

        public override string ToString() =>
            GUIPanel.StringSet.FilterWithTag(Entity.TagToString(tag));
    }

    public class MultipleTagFilter : Filter
    {
        public byte tagBits;  // bitfield

        public MultipleTagFilter() { } // deserialization

        public MultipleTagFilter(byte tagBits)
        {
            this.tagBits = tagBits;
        }

        public bool EntityMatches(EntityComponent entityComponent)
        {
            if (entityComponent == null)
                return false;
            return ((1 << entityComponent.entity.tag) & tagBits) != 0;
        }

        public override string ToString()
        {
            if (tagBits == 0)
                return GUIPanel.StringSet.FilterNothing;
            else if (tagBits == 255)
                return GUIPanel.StringSet.FilterAnything;
            string str = "";
            int count = 0;
            for (byte i = 0; i < 8; i++)
            {
                if ((tagBits & (1 << i)) != 0)
                {
                    str += Entity.TagToString(i);
                    count++;
                }
            }
            if (count == 1)
                return GUIPanel.StringSet.FilterWithTag(str);
            else
                return GUIPanel.StringSet.FilterMultipleTags(str);
        }
    }

    public Filter filter = new EntityTypeFilter(Entity.objectType);

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("fil", "Filter",
                () => filter,
                v => filter = (Filter)v,
                PropertyGUIs.Filter,
                true) // explicit type
        }, base.Properties());
}
