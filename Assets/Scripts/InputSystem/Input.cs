//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.5.1
//     from Assets/Scripts/InputSystem/Input.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @Input: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Input()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Input"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""a46ef0a5-b64c-44f7-a675-383290fc786e"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""4e2285dd-374e-4361-8535-1735535c77bb"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""362c48a0-3f18-4cb1-a99d-f5380fce0b64"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""ab36b41a-6873-4aaf-9807-fa3c4178aeb9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""PassThrough"",
                    ""id"": ""81345438-6cb7-49d6-bdbf-69f148981026"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Rotation"",
                    ""type"": ""Value"",
                    ""id"": ""bc0f839f-8cf5-4dd2-b7d0-16818059266d"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""4b91d505-885e-49cf-bdd8-619c47e3ba77"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MouseRightDown"",
                    ""type"": ""Button"",
                    ""id"": ""be921264-c9ab-439b-bb2b-e0c61d132501"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseRightUp"",
                    ""type"": ""Button"",
                    ""id"": ""256febb3-44d1-41d9-be91-ccd75c696b1f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseLeftDown"",
                    ""type"": ""Button"",
                    ""id"": ""00bd79b7-85fc-4c1d-88de-5de43e6bfde6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseLeftUp"",
                    ""type"": ""Button"",
                    ""id"": ""9e5e453c-4591-4d75-ba10-6b7ac381e0dc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ShiftKey"",
                    ""type"": ""Button"",
                    ""id"": ""1bd4a543-b433-4cd7-9405-d962a6b68a58"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AKey"",
                    ""type"": ""Button"",
                    ""id"": ""4dcee683-b44b-44d8-9cfb-61447d7b3208"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""0Key"",
                    ""type"": ""Button"",
                    ""id"": ""4cf4e75a-7e3c-4413-9051-8c00a853e981"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""1Key"",
                    ""type"": ""Button"",
                    ""id"": ""a12e009a-da07-46fb-9410-18bae2264253"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""2Key"",
                    ""type"": ""Button"",
                    ""id"": ""814e54e8-8df9-4cec-a7a9-2d2892f64a0f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""3Key"",
                    ""type"": ""Button"",
                    ""id"": ""83e003fd-2164-4226-bf96-a6cc9acb0ef8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""4Key"",
                    ""type"": ""Button"",
                    ""id"": ""7197ce98-304f-41e8-9ce0-fd7bf1ed7006"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""5Key"",
                    ""type"": ""Button"",
                    ""id"": ""f55f4fc2-f60f-47ca-897a-98746dc61fff"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""6Key"",
                    ""type"": ""Button"",
                    ""id"": ""b4be6fbe-52ee-461d-a578-5e4e0aa2b66e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""7Key"",
                    ""type"": ""Button"",
                    ""id"": ""b6400cdb-6218-4fcb-b3c7-0a30caaa329d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""8Key"",
                    ""type"": ""Button"",
                    ""id"": ""d3b8106c-06e5-4ffe-95fe-ffd6e5a272a4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""9Key"",
                    ""type"": ""Button"",
                    ""id"": ""a8be920c-cbc0-401e-bda1-cd6d64f9a9d1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""4eb83111-bab1-4bfd-ad81-6a032db4c92a"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""8f989359-020e-419c-b773-a2c69eb51db3"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""e37f3a90-609b-4b01-a7df-20eb59cd50af"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""8d381b85-fba7-4ede-8e93-56233c28c8f1"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8bc5b77e-7d6c-4662-b09f-fbef7316db22"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""8f9fa472-2b72-4fd7-8e8b-abba8870e0d5"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9b8716f3-6477-45b9-ba4b-2e90d606cecf"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5b6cf9bd-aeac-44e1-82b2-e6424bb706db"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0444854f-877b-49c7-9cfd-b1b6fbb323fd"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e137bf5f-32ee-48c0-ba3b-40f04f974820"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cf422b07-90ca-46a2-8c19-3ff22e846fa1"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseRightDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f9409b97-a37e-41d4-969f-52da1887016e"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ShiftKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""976f7484-0b7e-431f-8476-f1d6d316b5a5"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""89362dd9-8ea8-475c-856d-f4c63a3d5ba0"",
                    ""path"": ""<Keyboard>/0"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""0Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""12a096ef-e9da-4c48-ac2e-af3368aac883"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""1Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""86bc04d4-3a3f-4b4b-be4f-b70a0cfdaa3f"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""2Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""83c4494f-4018-437c-a320-53a1246bd361"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""3Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5160194b-af67-4997-8b3b-710e419634c4"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""4Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d8eab27a-d5b6-45a5-b7ee-99d5d053e593"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""5Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6fea5bb0-2661-4928-a8da-771b2eddc754"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""6Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7dce4150-a27d-429f-8e6d-b0ed3a7df01f"",
                    ""path"": ""<Keyboard>/7"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""7Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9a02bee5-d59c-47b4-a9e2-fabc044648f3"",
                    ""path"": ""<Keyboard>/8"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""8Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""555cd75a-2b48-47ff-83c1-f16b04a07005"",
                    ""path"": ""<Keyboard>/9"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""9Key"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""74755d24-ffee-4d4b-97c8-f62f5a307e51"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseLeftDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""339ffb7f-e840-4739-9a0d-70ec4876b08f"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseLeftUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""270593cf-53e2-4ec6-8086-8966b6bf185b"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseRightUp"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_Crouch = m_Player.FindAction("Crouch", throwIfNotFound: true);
        m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
        m_Player_Rotation = m_Player.FindAction("Rotation", throwIfNotFound: true);
        m_Player_MousePosition = m_Player.FindAction("MousePosition", throwIfNotFound: true);
        m_Player_MouseRightDown = m_Player.FindAction("MouseRightDown", throwIfNotFound: true);
        m_Player_MouseRightUp = m_Player.FindAction("MouseRightUp", throwIfNotFound: true);
        m_Player_MouseLeftDown = m_Player.FindAction("MouseLeftDown", throwIfNotFound: true);
        m_Player_MouseLeftUp = m_Player.FindAction("MouseLeftUp", throwIfNotFound: true);
        m_Player_ShiftKey = m_Player.FindAction("ShiftKey", throwIfNotFound: true);
        m_Player_AKey = m_Player.FindAction("AKey", throwIfNotFound: true);
        m_Player__0Key = m_Player.FindAction("0Key", throwIfNotFound: true);
        m_Player__1Key = m_Player.FindAction("1Key", throwIfNotFound: true);
        m_Player__2Key = m_Player.FindAction("2Key", throwIfNotFound: true);
        m_Player__3Key = m_Player.FindAction("3Key", throwIfNotFound: true);
        m_Player__4Key = m_Player.FindAction("4Key", throwIfNotFound: true);
        m_Player__5Key = m_Player.FindAction("5Key", throwIfNotFound: true);
        m_Player__6Key = m_Player.FindAction("6Key", throwIfNotFound: true);
        m_Player__7Key = m_Player.FindAction("7Key", throwIfNotFound: true);
        m_Player__8Key = m_Player.FindAction("8Key", throwIfNotFound: true);
        m_Player__9Key = m_Player.FindAction("9Key", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Player
    private readonly InputActionMap m_Player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Crouch;
    private readonly InputAction m_Player_Sprint;
    private readonly InputAction m_Player_Rotation;
    private readonly InputAction m_Player_MousePosition;
    private readonly InputAction m_Player_MouseRightDown;
    private readonly InputAction m_Player_MouseRightUp;
    private readonly InputAction m_Player_MouseLeftDown;
    private readonly InputAction m_Player_MouseLeftUp;
    private readonly InputAction m_Player_ShiftKey;
    private readonly InputAction m_Player_AKey;
    private readonly InputAction m_Player__0Key;
    private readonly InputAction m_Player__1Key;
    private readonly InputAction m_Player__2Key;
    private readonly InputAction m_Player__3Key;
    private readonly InputAction m_Player__4Key;
    private readonly InputAction m_Player__5Key;
    private readonly InputAction m_Player__6Key;
    private readonly InputAction m_Player__7Key;
    private readonly InputAction m_Player__8Key;
    private readonly InputAction m_Player__9Key;
    public struct PlayerActions
    {
        private @Input m_Wrapper;
        public PlayerActions(@Input wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @Crouch => m_Wrapper.m_Player_Crouch;
        public InputAction @Sprint => m_Wrapper.m_Player_Sprint;
        public InputAction @Rotation => m_Wrapper.m_Player_Rotation;
        public InputAction @MousePosition => m_Wrapper.m_Player_MousePosition;
        public InputAction @MouseRightDown => m_Wrapper.m_Player_MouseRightDown;
        public InputAction @MouseRightUp => m_Wrapper.m_Player_MouseRightUp;
        public InputAction @MouseLeftDown => m_Wrapper.m_Player_MouseLeftDown;
        public InputAction @MouseLeftUp => m_Wrapper.m_Player_MouseLeftUp;
        public InputAction @ShiftKey => m_Wrapper.m_Player_ShiftKey;
        public InputAction @AKey => m_Wrapper.m_Player_AKey;
        public InputAction @_0Key => m_Wrapper.m_Player__0Key;
        public InputAction @_1Key => m_Wrapper.m_Player__1Key;
        public InputAction @_2Key => m_Wrapper.m_Player__2Key;
        public InputAction @_3Key => m_Wrapper.m_Player__3Key;
        public InputAction @_4Key => m_Wrapper.m_Player__4Key;
        public InputAction @_5Key => m_Wrapper.m_Player__5Key;
        public InputAction @_6Key => m_Wrapper.m_Player__6Key;
        public InputAction @_7Key => m_Wrapper.m_Player__7Key;
        public InputAction @_8Key => m_Wrapper.m_Player__8Key;
        public InputAction @_9Key => m_Wrapper.m_Player__9Key;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Jump.started += instance.OnJump;
            @Jump.performed += instance.OnJump;
            @Jump.canceled += instance.OnJump;
            @Crouch.started += instance.OnCrouch;
            @Crouch.performed += instance.OnCrouch;
            @Crouch.canceled += instance.OnCrouch;
            @Sprint.started += instance.OnSprint;
            @Sprint.performed += instance.OnSprint;
            @Sprint.canceled += instance.OnSprint;
            @Rotation.started += instance.OnRotation;
            @Rotation.performed += instance.OnRotation;
            @Rotation.canceled += instance.OnRotation;
            @MousePosition.started += instance.OnMousePosition;
            @MousePosition.performed += instance.OnMousePosition;
            @MousePosition.canceled += instance.OnMousePosition;
            @MouseRightDown.started += instance.OnMouseRightDown;
            @MouseRightDown.performed += instance.OnMouseRightDown;
            @MouseRightDown.canceled += instance.OnMouseRightDown;
            @MouseRightUp.started += instance.OnMouseRightUp;
            @MouseRightUp.performed += instance.OnMouseRightUp;
            @MouseRightUp.canceled += instance.OnMouseRightUp;
            @MouseLeftDown.started += instance.OnMouseLeftDown;
            @MouseLeftDown.performed += instance.OnMouseLeftDown;
            @MouseLeftDown.canceled += instance.OnMouseLeftDown;
            @MouseLeftUp.started += instance.OnMouseLeftUp;
            @MouseLeftUp.performed += instance.OnMouseLeftUp;
            @MouseLeftUp.canceled += instance.OnMouseLeftUp;
            @ShiftKey.started += instance.OnShiftKey;
            @ShiftKey.performed += instance.OnShiftKey;
            @ShiftKey.canceled += instance.OnShiftKey;
            @AKey.started += instance.OnAKey;
            @AKey.performed += instance.OnAKey;
            @AKey.canceled += instance.OnAKey;
            @_0Key.started += instance.On_0Key;
            @_0Key.performed += instance.On_0Key;
            @_0Key.canceled += instance.On_0Key;
            @_1Key.started += instance.On_1Key;
            @_1Key.performed += instance.On_1Key;
            @_1Key.canceled += instance.On_1Key;
            @_2Key.started += instance.On_2Key;
            @_2Key.performed += instance.On_2Key;
            @_2Key.canceled += instance.On_2Key;
            @_3Key.started += instance.On_3Key;
            @_3Key.performed += instance.On_3Key;
            @_3Key.canceled += instance.On_3Key;
            @_4Key.started += instance.On_4Key;
            @_4Key.performed += instance.On_4Key;
            @_4Key.canceled += instance.On_4Key;
            @_5Key.started += instance.On_5Key;
            @_5Key.performed += instance.On_5Key;
            @_5Key.canceled += instance.On_5Key;
            @_6Key.started += instance.On_6Key;
            @_6Key.performed += instance.On_6Key;
            @_6Key.canceled += instance.On_6Key;
            @_7Key.started += instance.On_7Key;
            @_7Key.performed += instance.On_7Key;
            @_7Key.canceled += instance.On_7Key;
            @_8Key.started += instance.On_8Key;
            @_8Key.performed += instance.On_8Key;
            @_8Key.canceled += instance.On_8Key;
            @_9Key.started += instance.On_9Key;
            @_9Key.performed += instance.On_9Key;
            @_9Key.canceled += instance.On_9Key;
        }

        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Jump.started -= instance.OnJump;
            @Jump.performed -= instance.OnJump;
            @Jump.canceled -= instance.OnJump;
            @Crouch.started -= instance.OnCrouch;
            @Crouch.performed -= instance.OnCrouch;
            @Crouch.canceled -= instance.OnCrouch;
            @Sprint.started -= instance.OnSprint;
            @Sprint.performed -= instance.OnSprint;
            @Sprint.canceled -= instance.OnSprint;
            @Rotation.started -= instance.OnRotation;
            @Rotation.performed -= instance.OnRotation;
            @Rotation.canceled -= instance.OnRotation;
            @MousePosition.started -= instance.OnMousePosition;
            @MousePosition.performed -= instance.OnMousePosition;
            @MousePosition.canceled -= instance.OnMousePosition;
            @MouseRightDown.started -= instance.OnMouseRightDown;
            @MouseRightDown.performed -= instance.OnMouseRightDown;
            @MouseRightDown.canceled -= instance.OnMouseRightDown;
            @MouseRightUp.started -= instance.OnMouseRightUp;
            @MouseRightUp.performed -= instance.OnMouseRightUp;
            @MouseRightUp.canceled -= instance.OnMouseRightUp;
            @MouseLeftDown.started -= instance.OnMouseLeftDown;
            @MouseLeftDown.performed -= instance.OnMouseLeftDown;
            @MouseLeftDown.canceled -= instance.OnMouseLeftDown;
            @MouseLeftUp.started -= instance.OnMouseLeftUp;
            @MouseLeftUp.performed -= instance.OnMouseLeftUp;
            @MouseLeftUp.canceled -= instance.OnMouseLeftUp;
            @ShiftKey.started -= instance.OnShiftKey;
            @ShiftKey.performed -= instance.OnShiftKey;
            @ShiftKey.canceled -= instance.OnShiftKey;
            @AKey.started -= instance.OnAKey;
            @AKey.performed -= instance.OnAKey;
            @AKey.canceled -= instance.OnAKey;
            @_0Key.started -= instance.On_0Key;
            @_0Key.performed -= instance.On_0Key;
            @_0Key.canceled -= instance.On_0Key;
            @_1Key.started -= instance.On_1Key;
            @_1Key.performed -= instance.On_1Key;
            @_1Key.canceled -= instance.On_1Key;
            @_2Key.started -= instance.On_2Key;
            @_2Key.performed -= instance.On_2Key;
            @_2Key.canceled -= instance.On_2Key;
            @_3Key.started -= instance.On_3Key;
            @_3Key.performed -= instance.On_3Key;
            @_3Key.canceled -= instance.On_3Key;
            @_4Key.started -= instance.On_4Key;
            @_4Key.performed -= instance.On_4Key;
            @_4Key.canceled -= instance.On_4Key;
            @_5Key.started -= instance.On_5Key;
            @_5Key.performed -= instance.On_5Key;
            @_5Key.canceled -= instance.On_5Key;
            @_6Key.started -= instance.On_6Key;
            @_6Key.performed -= instance.On_6Key;
            @_6Key.canceled -= instance.On_6Key;
            @_7Key.started -= instance.On_7Key;
            @_7Key.performed -= instance.On_7Key;
            @_7Key.canceled -= instance.On_7Key;
            @_8Key.started -= instance.On_8Key;
            @_8Key.performed -= instance.On_8Key;
            @_8Key.canceled -= instance.On_8Key;
            @_9Key.started -= instance.On_9Key;
            @_9Key.performed -= instance.On_9Key;
            @_9Key.canceled -= instance.On_9Key;
        }

        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnRotation(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
        void OnMouseRightDown(InputAction.CallbackContext context);
        void OnMouseRightUp(InputAction.CallbackContext context);
        void OnMouseLeftDown(InputAction.CallbackContext context);
        void OnMouseLeftUp(InputAction.CallbackContext context);
        void OnShiftKey(InputAction.CallbackContext context);
        void OnAKey(InputAction.CallbackContext context);
        void On_0Key(InputAction.CallbackContext context);
        void On_1Key(InputAction.CallbackContext context);
        void On_2Key(InputAction.CallbackContext context);
        void On_3Key(InputAction.CallbackContext context);
        void On_4Key(InputAction.CallbackContext context);
        void On_5Key(InputAction.CallbackContext context);
        void On_6Key(InputAction.CallbackContext context);
        void On_7Key(InputAction.CallbackContext context);
        void On_8Key(InputAction.CallbackContext context);
        void On_9Key(InputAction.CallbackContext context);
    }
}
