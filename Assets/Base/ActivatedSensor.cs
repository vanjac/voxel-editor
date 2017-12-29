using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public abstract class ActivatedSensor : Sensor
{
    public class Filter
    {
        public enum Mode : byte
        {
            ENTITY, ENTITY_TYPE, TAG
        }

        public Mode mode;

        public EntityReference entityRef;
        public byte tag;

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

        public Filter SetEntity(Entity e)
        {
            mode = Mode.ENTITY;
            entityRef = new EntityReference(e);
            entityType = null;
            tag = 0;
            return this;
        }

        public Filter SetEntityType(PropertiesObjectType type)
        {
            mode = Mode.ENTITY_TYPE;
            entityRef = new EntityReference(null);
            entityType = type;
            tag = 0;
            return this;
        }

        public Filter SetTag(byte t)
        {
            mode = Mode.TAG;
            entityRef = new EntityReference(null);
            entityType = null;
            tag = t;
            return this;
        }

        public bool EntityMatches(EntityComponent entityComponent)
        {
            Entity e = entityComponent.entity;
            switch (mode)
            {
                case Mode.ENTITY:
                    return e == entityRef.entity;
                case Mode.ENTITY_TYPE:
                    return entityType.type.IsInstanceOfType(e); // TODO: check behaviors
                case Mode.TAG:
                    return e.tag == tag;
            }
            return true;
        }

        public override string ToString()
        {
            switch (mode)
            {
                case Mode.ENTITY:
                    return entityRef.entity.ToString();
                case Mode.ENTITY_TYPE:
                    return entityType.fullName;
                case Mode.TAG:
                    return Entity.TagToString(tag);
            }
            return "Filter";
        }
    }

    private Filter filter = new Filter().SetEntityType(Entity.objectType);

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
