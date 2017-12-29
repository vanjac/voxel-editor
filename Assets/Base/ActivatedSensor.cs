using System.Collections;
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
            return entityComponent.entity == entityRef.entity;
        }

        public override string ToString()
        {
            return "Only " + entityRef.entity.ToString();
        }
    }

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
                        _entityType = instance;
                        return _entityType;
                    }
                    Debug.Log("Couldn't find matching filter type for " + _entityType.fullName + "!");
                    _entityType = null;
                }
                return _entityType;
            }
            set
            {
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
            return entityType.type.IsInstanceOfType(entityComponent.entity); // TODO: check behaviors
        }

        public override string ToString()
        {
            return entityType.fullName;
        }
    }

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
            return entityComponent.entity.tag == tag;
        }

        public override string ToString()
        {
            return "With tag " + Entity.TagToString(tag);
        }
    }

    private Filter filter = new EntityTypeFilter(Entity.objectType);

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Filter",
                () => filter,
                v => filter = (Filter)v,
                PropertyGUIs.Filter)
        }, base.Properties());
    }
}
