using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Entity
{
    string EntityTypeName();

    bool SupportsTag();
    byte GetTag();
    void SetTag(byte tag);

    string[] PropertyNames();
    string GetProperty(string name);
    void SetProperty(string name, string value);
    string PropertyGUI(string name, string value);

    string[] ActionNames();
    string ActionArgumentGUI(string name, string value);

    string[] EventNames();
    bool EventHasActivator();

    List<Output> OutputList(); // can be null if outputs not supported

    List<Entity> BehaviorList(); // can be null if behaviors not supported
}

public struct Output
{
    Entity targetEntity; // null for Self or Activator
    bool targetEntityIsActivator;
    string targetAction;
    string actionArgument;

    // activator rule...
    bool[] activatorTagsAllowed;
    Entity[] activatorEntityList;
    bool activatorEntityBlacklist;
    string[] activatorTypeList; // also applies to behaviors
    bool activatorTypeBlacklist;
}
