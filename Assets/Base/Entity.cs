using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public delegate void PropertyGUI(Property property);

public struct Property {
    public string id;
    public Localizer name;
    public Func<object> getter;
    public Action<object> setter;
    public PropertyGUI gui;
    public object value {
        get => getter();
        set {
            if (!getter().Equals(value)) {
                setter(value);
            }
        }
    }
    public bool explicitType; // store object type with property in file

    public Property(string id, Localizer name, Func<object> getter, Action<object> setter,
            PropertyGUI gui, bool explicitType = false) {
        this.id = id;
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.gui = gui;
        this.explicitType = explicitType;
    }

    public static IEnumerable<Property> JoinProperties(
        IEnumerable<Property> props1, IEnumerable<Property> props2) => props1.Concat(props2);
}


// needs to support serialization since it is a part of ActivatedSensor.Filter
// PropertiesObjectType is only serialized with its full name
// after it is deserialized, it is replaced with the correct instance of PropertiesObjectType
// with all the missing data (this is done by Filter)
public class PropertiesObjectType {
    public static readonly PropertiesObjectType NONE = new PropertiesObjectType("None", null) {
        displayName = s => s.NoneName,
        iconName = "cancel",
    };

    // This name is used for identifying types and for error messages if a type is not recognized.
    // It is always in English (not localized) and can never change.
    public string fullName;

    [XmlIgnore]
    public readonly Type type;
    [XmlIgnore]
    public Func<PropertiesObject> constructor;

    [XmlIgnore]
    public Localizer displayName;
    [XmlIgnore]
    public Localizer description = GUIStringSet.Empty;
    [XmlIgnore]
    public Localizer longDescription = GUIStringSet.Empty;
    [XmlIgnore]
    public string iconName = "";

    private Texture _icon;
    public Texture icon {
        get {
            if (_icon == null && iconName.Length > 0) {
                _icon = Resources.Load<Texture>("Icons/" + iconName);
            }
            return _icon;
        }
    }

    // empty constructor for deserialization
    public PropertiesObjectType() {
        constructor = DefaultConstructor;
        displayName = DefaultDisplayName;
    }

    public PropertiesObjectType(string fullName, Type type) {
        this.fullName = fullName;
        this.type = type;
        constructor = DefaultConstructor;
        displayName = DefaultDisplayName;
    }

    public PropertiesObjectType(PropertiesObjectType baseType, Func<PropertiesObject> newConstructor) {
        fullName = baseType.fullName;
        displayName = baseType.displayName;
        description = baseType.description;
        longDescription = baseType.longDescription;
        iconName = baseType.iconName;
        type = baseType.type;
        constructor = newConstructor;
    }

    private string DefaultDisplayName(GUIStringSet s) => fullName;

    private PropertiesObject DefaultConstructor() {
        if (type == null) {
            return null;
        }
        return (PropertiesObject)Activator.CreateInstance(type);
    }

    public PropertiesObject Create() => constructor();

    // assumes both objects are the same type and have the same order of properties
    public static void CopyProperties(PropertiesObject source, PropertiesObject dest,
            Entity findEntity = null, Entity replaceEntity = null) {
        var sourceEnumerator = source.Properties().GetEnumerator();
        var destEnumerator = dest.Properties().GetEnumerator();
        while (sourceEnumerator.MoveNext()) {
            destEnumerator.MoveNext();
            var value = sourceEnumerator.Current.value;
            // change "Self" references...
            value = PropertyValueReplaceEntity(value, findEntity, replaceEntity);
            destEnumerator.Current.setter(value);
        }
    }

    public static System.Object PropertyValueReplaceEntity(System.Object value,
            Entity findEntity, Entity replaceEntity) {
        if (findEntity == null) {
            return value;
        }
        if (value is EntityReference entityRef) {
            if (entityRef.entity == findEntity) {
                return new EntityReference(replaceEntity);
            }
        } else if (value is Target target) {
            if (target.entityRef.entity == findEntity) {
                return new Target(replaceEntity);
            }
        }
        return value;
    }

    public static object GetProperty(PropertiesObject obj, string key) {
        foreach (Property prop in obj.Properties()) {
            if (prop.id == key) {
                return prop.getter();
            }
        }
        return null;
    }

    public static bool SetProperty(PropertiesObject obj, string key, object value) {
        foreach (Property prop in obj.Properties()) {
            if (prop.id == key) {
                prop.setter(value);
                return true;
            }
        }
        return false;
    }
}

public interface PropertiesObject {
    PropertiesObjectType ObjectType { get; }
    IEnumerable<Property> Properties();
    IEnumerable<Property> DeprecatedProperties();
}

public abstract class Entity : PropertiesObject {
    public static PropertiesObjectType objectType = new PropertiesObjectType(
            "Anything", typeof(Entity)) {
        displayName = s => s.AnythingName,
        iconName = "circle-outline",
    };

    public EntityComponent component;
    public Sensor sensor;
    public List<EntityBehavior> behaviors = new List<EntityBehavior>();
    public byte tag = 0;
    public const byte NUM_TAGS = 8;

    public Guid guid = Guid.Empty; // set by EntityReference

    public static string TagToString(byte tag) {
        // interesting unicode symbols start at U+25A0
        // U+2700 symbols don't seem to work
        return "■▲●★♥♦♠♣".Substring(tag, 1);
    }

    public override string ToString() => TagToString(tag) + " " + ObjectType.fullName;
    public string ToString(GUIStringSet s) =>
        TagToString(tag) + " " + ObjectType.displayName(s);

    public virtual PropertiesObjectType ObjectType => objectType;

    public virtual IEnumerable<Property> Properties() =>
        new Property[] {
            new Property("tag", s => s.PropTag,
                () => tag,
                v => tag = (byte)v,
                PropertyGUIs.Tag),
        };

    public virtual IEnumerable<Property> DeprecatedProperties() => Array.Empty<Property>();

    // storeComponent: whether the "component" variable should be set
    // this should usually be true, but false for clones
    public abstract EntityComponent InitEntityGameObject(VoxelArray voxelArray, bool storeComponent = true);

    public abstract Vector3 PositionInEditor();
    public abstract bool AliveInEditor();
    public abstract void SetHighlight(Color c);

    public virtual Entity Clone() {
        var newEntity = (Entity)ObjectType.Create();
        PropertiesObjectType.CopyProperties(this, newEntity, this, newEntity);
        if (sensor != null) {
            newEntity.sensor = (Sensor)sensor.ObjectType.Create();
            PropertiesObjectType.CopyProperties(sensor, newEntity.sensor, this, newEntity);
        } else {
            newEntity.sensor = null; // in case the Object Type had a default sensor
        }
        newEntity.behaviors.Clear(); // in case the Object Type had default behaviors
        foreach (var behavior in behaviors) {
            var newBehavior = (EntityBehavior)behavior.ObjectType.Create();
            PropertiesObjectType.CopyProperties(behavior, newBehavior, this, newEntity);
            newEntity.behaviors.Add(newBehavior);
        }
        return newEntity;
    }
}

public abstract class EntityComponent : MonoBehaviour {
    public Entity entity;

    private List<Behaviour> offComponents = new List<Behaviour>();
    private List<Behaviour> onComponents = new List<Behaviour>();
    private List<Behaviour> targetedComponents = new List<Behaviour>();
    private List<EntityBehavior> activatorBehaviors = new List<EntityBehavior>();

    private Dictionary<EntityComponent, List<Behaviour>> activatorComponents
        = new Dictionary<EntityComponent, List<Behaviour>>();

    private ISensorComponent sensorComponent;
    private bool sensorWasOn;

    public static EntityComponent FindEntityComponent(GameObject obj) {
        EntityComponent component = obj.GetComponent<EntityComponent>();
        if (component != null) {
            return component;
        }
        Transform parent = obj.transform.parent;
        if (parent != null) {
            return parent.GetComponent<EntityComponent>();
        }
        return null;
    }

    public static EntityComponent FindEntityComponent(Component c) =>
        FindEntityComponent(c.gameObject);

    public virtual void Start() {
        if (entity.sensor != null) {
            sensorComponent = entity.sensor.MakeComponent(gameObject);
        }
        sensorWasOn = false;
        foreach (EntityBehavior behavior in entity.behaviors) {
            if (behavior.targetEntityIsActivator) {
                activatorBehaviors.Add(behavior);
                continue;
            }

            Behaviour c;
            if (behavior.targetEntity.entity != null) {
                c = behavior.MakeComponent(behavior.targetEntity.entity.component.gameObject);
                targetedComponents.Add(c);
            } else {
                c = behavior.MakeComponent(gameObject);
            }
            if (behavior.condition == EntityBehavior.Condition.OFF) {
                offComponents.Add(c);
                c.enabled = true;
            } else if (behavior.condition == EntityBehavior.Condition.ON) {
                onComponents.Add(c);
                c.enabled = false;
            } else {
                c.enabled = true;
            }
        }
    }

    void Update() {
        if (sensorComponent == null) {
            return;
        }
        bool sensorIsOn = IsOn();
        // order is important. behaviors should be disabled before other behaviors are enabled,
        // especially if identical behaviors are being disabled/enabled
        if (sensorIsOn && !sensorWasOn) {
            foreach (Behaviour offComponent in offComponents) {
                if (offComponent != null) {
                    offComponent.enabled = false;
                }
            }
            foreach (Behaviour onComponent in onComponents) {
                if (onComponent != null) {
                    onComponent.enabled = true;
                }
            }
        } else if (!sensorIsOn && sensorWasOn) {
            foreach (Behaviour onComponent in onComponents) {
                if (onComponent != null) {
                    onComponent.enabled = false;
                }
            }
            foreach (Behaviour offComponent in offComponents) {
                if (offComponent != null) {
                    offComponent.enabled = true;
                }
            }
        }
        sensorWasOn = sensorIsOn;


        foreach (EntityComponent newActivator in GetNewActivators()) {
            if (newActivator == null) {
                continue;
            }
            var behaviorComponents = new List<Behaviour>();
            foreach (EntityBehavior behavior in activatorBehaviors) {
                if (!behavior.BehaviorObjectType.rule(newActivator.entity)) {
                    continue;
                }
                Behaviour c = behavior.MakeComponent(newActivator.gameObject);
                behaviorComponents.Add(c);
                c.enabled = true;
            }
            activatorComponents[newActivator] = behaviorComponents;
        }
        foreach (EntityComponent removedActivator in GetRemovedActivators()) {
            if (removedActivator == null) {
                continue;
            }
            try {
                var behaviorComponents = activatorComponents[removedActivator];
                foreach (Behaviour b in behaviorComponents) {
                    if (b != null) {
                        Destroy(b);
                    }
                }
            } catch (KeyNotFoundException) { }
            activatorComponents.Remove(removedActivator);
        }
    }

    void OnDestroy() {
        foreach (Behaviour c in targetedComponents) {
            if (c != null) {
                Destroy(c);
            }
        }
        foreach (List<Behaviour> behaviorComponents in activatorComponents.Values) {
            foreach (Behaviour c in behaviorComponents) {
                if (c != null) {
                    Destroy(c);
                }
            }
        }
    }

    public bool IsOn() {
        if (sensorComponent == null) {
            return false;
        }
        return sensorComponent.IsOn();
    }

    public ICollection<EntityComponent> GetActivators() {
        if (sensorComponent == null) {
            return Array.Empty<EntityComponent>();
        }
        return sensorComponent.GetActivators();
    }

    public ICollection<EntityComponent> GetNewActivators() {
        if (sensorComponent == null) {
            return Array.Empty<EntityComponent>();
        }
        return sensorComponent.GetNewActivators();
    }

    public ICollection<EntityComponent> GetRemovedActivators() {
        if (sensorComponent == null) {
            return Array.Empty<EntityComponent>();
        }
        return sensorComponent.GetRemovedActivators();
    }
}

public abstract class EntityBehavior : PropertiesObject {
    public static BehaviorType objectType = new BehaviorType(
        "Behavior", typeof(EntityBehavior));

    public enum Condition : byte {
        ON = 0, OFF = 1, BOTH = 2
    }
    public struct BehaviorTargetProperty {
        public EntityReference targetEntity;
        public bool targetEntityIsActivator;
        public BehaviorTargetProperty(EntityReference targetEntity, bool targetEntityIsActivator) {
            this.targetEntity = targetEntity;
            this.targetEntityIsActivator = targetEntityIsActivator;
        }
    }

    public Condition condition = Condition.BOTH;
    public EntityReference targetEntity = new EntityReference(null); // null for self
    public bool targetEntityIsActivator = false;

    public PropertiesObjectType ObjectType => BehaviorObjectType;
    public virtual BehaviorType BehaviorObjectType => objectType;

    public virtual IEnumerable<Property> Properties() =>
        new Property[] {
            new Property("tar", s => s.PropTarget,
                () => new BehaviorTargetProperty(targetEntity, targetEntityIsActivator),
                v => {
                    var prop = (BehaviorTargetProperty)v;

                    // selfEntity will be null if multiple entities are selected
                    Entity selfEntity = EntityReferencePropertyManager.CurrentEntity();

                    var oldTargetEntity = targetEntity.entity;
                    var newTargetEntity = prop.targetEntity.entity;
                    if (oldTargetEntity == null && !targetEntityIsActivator) {
                        oldTargetEntity = selfEntity;
                    }
                    if (newTargetEntity == null && !prop.targetEntityIsActivator) {
                        newTargetEntity = selfEntity;
                    }
                    if (oldTargetEntity != null) {
                        // replace all property values referencing the old target with the new target
                        // the new target could be null
                        foreach (Property _selfProp in this.Properties()) {
                            var selfProp = _selfProp;
                            selfProp.value = PropertiesObjectType.PropertyValueReplaceEntity(
                                selfProp.value, oldTargetEntity, newTargetEntity);
                        }
                    }

                    targetEntity = prop.targetEntity;
                    targetEntityIsActivator = prop.targetEntityIsActivator;
                },
                PropertyGUIs.BehaviorTarget),
            new Property("con", s => s.PropCondition,
                () => condition,
                v => condition = (Condition)v,
                (Property property) => {
                    if (targetEntityIsActivator) {
                        PropertyGUIs.ActivatorBehaviorCondition(property);
                    } else {
                        PropertyGUIs.BehaviorCondition(property);
                    }
                })
        };

    public virtual IEnumerable<Property> DeprecatedProperties() => Array.Empty<Property>();

    public abstract Behaviour MakeComponent(GameObject gameObject);
}

public abstract class GenericEntityBehavior<SelfType, ComponentType> : EntityBehavior
        where SelfType : GenericEntityBehavior<SelfType, ComponentType>
        where ComponentType : BehaviorComponent<SelfType> {
    public override Behaviour MakeComponent(GameObject gameObject) {
        var component = gameObject.AddComponent<ComponentType>();
        component.Init((SelfType)this);
        return component;
    }
}


public abstract class BehaviorComponent<T> : MonoBehaviour {
    protected T behavior;
    private bool started = false;

    public virtual void Init(T behavior) {
        this.behavior = behavior;
    }

    // called after object is created and first enabled
    public virtual void Start() {
        started = true;
        if (enabled) {
            BehaviorEnabled();
        }
    }

    public virtual void OnEnable() {
        if (started) {
            BehaviorEnabled();
        }
    }

    public virtual void OnDisable() {
        if (started) {
            BehaviorDisabled();
            bool anyMatchingBehaviorsRemaining = false;
            foreach (Behaviour behavior in GetComponents(GetType())) {
                if (behavior.enabled) {
                    anyMatchingBehaviorsRemaining = true;
                    break;
                }
            }
            if (!anyMatchingBehaviorsRemaining) {
                LastBehaviorDisabled();
            }
        }
    }

    public virtual void BehaviorEnabled() { }
    public virtual void BehaviorDisabled() { }
    // called after BehaviorDisabled(), if there are no more instances of this behavior still enabled
    public virtual void LastBehaviorDisabled() { }
}


public class BehaviorType : PropertiesObjectType {
    public Predicate<Entity> rule = DefaultRule;

    public BehaviorType(string fullName, Type type)
        : base(fullName, type) { }

    private static bool DefaultRule(Entity checkEntity) => true;

    public static Predicate<Entity> AndRule(Predicate<Entity> r1, Predicate<Entity> r2) =>
        (Entity checkEntity) => r1(checkEntity) && r2(checkEntity);

    public static Predicate<Entity> BaseTypeRule(Type baseType) =>
        (Entity checkEntity) => baseType.IsAssignableFrom(checkEntity.GetType());

    public static Predicate<Entity> NotBaseTypeRule(Type baseType) =>
        (Entity checkEntity) => !baseType.IsAssignableFrom(checkEntity.GetType());
}


public abstract class Sensor : PropertiesObject {
    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "Sensor", typeof(Sensor));

    public virtual PropertiesObjectType ObjectType => objectType;

    public virtual IEnumerable<Property> Properties() => Array.Empty<Property>();

    public virtual IEnumerable<Property> DeprecatedProperties() => Array.Empty<Property>();

    public abstract ISensorComponent MakeComponent(GameObject gameObject);
}

public abstract class GenericSensor<SelfType, ComponentType> : Sensor
        where SelfType : GenericSensor<SelfType, ComponentType>
        where ComponentType : SensorComponent<SelfType> {
    public override ISensorComponent MakeComponent(GameObject gameObject) {
        var component = gameObject.AddComponent<ComponentType>();
        component.Init((SelfType)this);
        return component;
    }
}

public interface ISensorComponent {
    bool IsOn();
    void LateUpdate();
    ICollection<EntityComponent> GetActivators();
    ICollection<EntityComponent> GetNewActivators();
    ICollection<EntityComponent> GetRemovedActivators();
    void ClearActivators();
}

public abstract class SensorComponent<T> : MonoBehaviour, ISensorComponent {
    protected T sensor;

    private HashSet<EntityComponent> activators = new HashSet<EntityComponent>();

    private HashSet<EntityComponent> newActivators = new HashSet<EntityComponent>();
    protected HashSet<EntityComponent> newActivators_next = new HashSet<EntityComponent>();
    private HashSet<EntityComponent> removedActivators = new HashSet<EntityComponent>();
    protected HashSet<EntityComponent> removedActivators_next = new HashSet<EntityComponent>();

    public virtual void Init(T sensor) {
        this.sensor = sensor;
    }

    public bool IsOn() => GetActivators().Count > 0;

    public virtual void LateUpdate() {
        NewFrame();
    }

    // all current activators
    // if the number is greater than zero, the sensor is on
    // a null activator is possible - this allows the sensor to be on without having any activators
    public ICollection<EntityComponent> GetActivators() => activators;

    // activators that have been added this frame
    public ICollection<EntityComponent> GetNewActivators() => newActivators;

    // activators that have been removed this frame
    public ICollection<EntityComponent> GetRemovedActivators() => removedActivators;

    private void NewFrame() {
        activators.UnionWith(newActivators_next);
        activators.ExceptWith(removedActivators_next);

        // swap and clear
        var temp = newActivators;
        newActivators = newActivators_next;
        newActivators_next = temp;
        newActivators_next.Clear();

        temp = removedActivators;
        removedActivators = removedActivators_next;
        removedActivators_next = temp;
        removedActivators_next.Clear();
    }

    protected void AddActivator(EntityComponent activator) {
        // not short circuit
        if (!activators.Contains(activator) & !removedActivators_next.Remove(activator)) {
            newActivators_next.Add(activator);
        }
    }

    protected void AddActivators(ICollection<EntityComponent> activators) {
        // TODO: use boolean operations
        foreach (var activator in activators) {
            AddActivator(activator);
        }
    }

    protected void RemoveActivator(EntityComponent activator) {
        if (activators.Contains(activator) & !newActivators_next.Remove(activator)) {
            removedActivators_next.Add(activator);
        }
    }

    protected void RemoveActivators(ICollection<EntityComponent> activators) {
        // TODO: use boolean operations
        foreach (var activator in activators) {
            RemoveActivator(activator);
        }
    }

    public void ClearActivators() {
        removedActivators_next.UnionWith(activators);
        removedActivators_next.ExceptWith(newActivators_next);
        newActivators_next.Clear();
    }
}


public abstract class DynamicEntity : Entity {
    // only for editor; makes object transparent allowing you to zoom/select through it
    public bool xRay = false;
    public float health = 100;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[] {
            new Property("xra", s => s.PropXRay,
                () => xRay,
                v => {xRay = (bool)v; UpdateEntityEditor();},
                PropertyGUIs.Toggle),
            new Property("hea", s => s.PropHealth,
                () => health,
                v => health = (float)v,
                PropertyGUIs.Float)
        });

    // update the DynamicEntity's appearance in the Editor
    public virtual void UpdateEntityEditor() { }
}

public abstract class DynamicEntityComponent : EntityComponent {
    // TODO: this is really awful
    public static readonly Vector3 KILL_LOCATION = new Vector3(9999, 9999, 9999);

    public float health;
    public bool isCharacter = false;
    private Vector3 lastRigidbodyPosition;
    private Vector3 cumulativeRigidbodyTranslate;
    private Quaternion lastRigidbodyRotation;
    private Quaternion cumulativeRigidbodyRotate;

    public void Hurt(float amount) {
        health -= amount;
        if (health <= 0) {
            health = 0;
            Die();
        }
    }

    public void Heal(float amount) {
        health += amount;
    }

    public void Die() {
        // move entity out of any touch sensors so they will have a chance to turn off before it's destroyed
        transform.position = KILL_LOCATION;
        ISensorComponent sensor = GetComponent<ISensorComponent>();
        if (sensor != null) {
            // make sure activators are removed from any outputs
            sensor.ClearActivators();
        }
        StartCoroutine(DestroyCoroutine());
    }

    private IEnumerator DestroyCoroutine() {
        yield return null;
        Destroy(gameObject);
    }

    // allows composing multiple translations within a single FixedUpdate cycle
    // Rigidbody normally doesn't update its position until the end of the cycle
    public void RigidbodyTranslate(Rigidbody rb, Vector3 amount, bool applyConstraints = false) {
        if (rb.position != lastRigidbodyPosition) {
            // new FixedUpdate cycle
            lastRigidbodyPosition = rb.position;
            cumulativeRigidbodyTranslate = Vector3.zero;
        }

        cumulativeRigidbodyTranslate += amount;
        if (applyConstraints) {
            var constraints = RigidbodyConstraints.FreezeRotation;
            if (cumulativeRigidbodyTranslate.x == 0) {
                constraints |= RigidbodyConstraints.FreezePositionX;
            }
            if (cumulativeRigidbodyTranslate.y == 0) {
                constraints |= RigidbodyConstraints.FreezePositionY;
            }
            if (cumulativeRigidbodyTranslate.z == 0) {
                constraints |= RigidbodyConstraints.FreezePositionZ;
            }
            rb.constraints = constraints;
            rb.centerOfMass = Vector3.zero; // fix rotation pivot
        }
        rb.MovePosition(rb.position + cumulativeRigidbodyTranslate);
    }

    public void RigidbodyRotate(Rigidbody rb, Quaternion amount) {
        if (rb.rotation != lastRigidbodyRotation) {
            // new FixedUpdate cycle
            lastRigidbodyRotation = rb.rotation;
            cumulativeRigidbodyRotate = Quaternion.identity;
        }
        cumulativeRigidbodyRotate *= amount;
        rb.MoveRotation(rb.rotation * cumulativeRigidbodyRotate);
    }
}