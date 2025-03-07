using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets.Tests
{
    public class ThirdPersonControllerTests
    {
        private GameObject playerObject;
        private ThirdPersonController controller;
        private StarterAssetsInputs inputs;
        private CharacterController characterController;
        private Animator animator;
        private GameObject mainCamera;

        [SetUp]
        public void Setup()
        {
            // ��������� GameObject ��� ������
            playerObject = new GameObject("Player");
            playerObject.SetActive(true);

            // ������ ����������
            controller = playerObject.AddComponent<ThirdPersonController>();
            characterController = playerObject.AddComponent<CharacterController>();
            animator = playerObject.AddComponent<Animator>();
            inputs = playerObject.AddComponent<StarterAssetsInputs>();

            // ����������� ������� ������
            mainCamera = new GameObject("MainCamera");
            mainCamera.tag = "MainCamera";

            // ������������ �������� ��������
            controller.MoveSpeed = 2.0f;
            controller.SprintSpeed = 5.335f;
            controller.TopClamp = 70.0f;
            controller.BottomClamp = -30.0f;

            // ������ CinemachineCameraTarget
            controller.CinemachineCameraTarget = new GameObject("CinemachineTarget");

            // ������������ ������� ���� ����� ��������
            SetField("_threshold", 0.01f);
            SetField("_hasAnimator", true);
            SetField("_animator", animator);
            SetField("_controller", characterController);
            SetField("_input", inputs);
            SetField("_mainCamera", mainCamera);

#if ENABLE_INPUT_SYSTEM
            if (!playerObject.GetComponent<PlayerInput>())
            {
                var playerInput = playerObject.AddComponent<PlayerInput>();
                SetField("_playerInput", playerInput);
            }
#endif

            // ��������� Start() ��� �����������
            InvokeMethod("Start");

            // ���������� ������������
            Assert.IsNotNull(playerObject, "playerObject �� �� ���� null");
            Assert.IsNotNull(animator, "��������� Animator �� ���� �����������");
            Assert.IsNotNull(GetField("_animator"), "���� _animator �� ���� �����������");
            Assert.IsTrue((bool)GetField("_hasAnimator"), "_hasAnimator �� ���� true");
        }

        [TearDown]
        public void TearDown()
        {
            if (playerObject != null) Object.DestroyImmediate(playerObject);
            if (mainCamera != null) Object.DestroyImmediate(mainCamera);
            if (controller != null && controller.CinemachineCameraTarget != null) Object.DestroyImmediate(controller.CinemachineCameraTarget);
        }

        [Test]
        public void Move_SetsTargetSpeedBasedOnSprint()
        {
            // Arrange
            Assert.IsNotNull(GetField("_controller"), "_controller �� �� ���� null ����� Move");
            Assert.IsNotNull(GetField("_animator"), "_animator �� �� ���� null ����� Move");
            Assert.IsTrue((bool)GetField("_hasAnimator"), "_hasAnimator �� ���� true");

            SetField("_isDead", false);
            inputs.move = new Vector2(0f, 1f);
            inputs.sprint = true;
            SetField("_speed", 0f);
            SetField("_controller.velocity", Vector3.zero);

            // Act
            InvokeMethod("Move");

            // Assert
            float speed = (float)GetField("_speed");
            Assert.IsTrue(speed > 0f && speed <= controller.SprintSpeed, "�������� �� ���������� SprintSpeed �� ��� �������");
        }

        [Test]
        public void Move_DoesNotChangeSpeedWhenControllerDisabled()
        {
            // Arrange - ���������� �������: CharacterController ����������
            Assert.IsNotNull(GetField("_controller"), "_controller �� �� ���� null ����� Move");
            characterController.enabled = false; // �������� CharacterController
            SetField("_isDead", false);
            inputs.move = new Vector2(0f, 1f); // ������ ���
            inputs.sprint = true;
            SetField("_speed", 0f);
            SetField("_controller.velocity", Vector3.zero);

            // Act
            InvokeMethod("Move");

            // Assert - �������� �� ������� ��������
            float speed = (float)GetField("_speed");
            Assert.AreEqual(0f, speed, "�������� �� ������� ����������, ���� CharacterController ����������");
        }

        [Test]
        public void AssignAnimationIDs_SetsCorrectHashes()
        {
            // Arrange - ������� �������� ����� ������
            SetField("_animIDSpeed", 0);
            SetField("_animIDGrounded", 0);
            SetField("_animIDJump", 0);
            SetField("_animIDFreeFall", 0);
            SetField("_animIDMotionSpeed", 0);
            SetField("_animIDDeath", 0);

            // Act - ��������� ����� ����� ��������
            var method = typeof(ThirdPersonController).GetMethod("AssignAnimationIDs", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                Assert.Fail("����� AssignAnimationIDs �� ��������!");
                return;
            }
            method.Invoke(controller, null);

            // Assert - ����������, �� ��������� ���������� ����
            Assert.AreEqual(Animator.StringToHash("Speed"), GetField("_animIDSpeed"), "animIDSpeed �� ��������� ���� 'Speed'");
            Assert.AreEqual(Animator.StringToHash("Grounded"), GetField("_animIDGrounded"), "animIDGrounded �� ��������� ���� 'Grounded'");
            Assert.AreEqual(Animator.StringToHash("Jump"), GetField("_animIDJump"), "animIDJump �� ��������� ���� 'Jump'");
            Assert.AreEqual(Animator.StringToHash("FreeFall"), GetField("_animIDFreeFall"), "animIDFreeFall �� ��������� ���� 'FreeFall'");
            Assert.AreEqual(Animator.StringToHash("MotionSpeed"), GetField("_animIDMotionSpeed"), "animIDMotionSpeed �� ��������� ���� 'MotionSpeed'");
            Assert.AreEqual(Animator.StringToHash("Death"), GetField("_animIDDeath"), "animIDDeath �� ��������� ���� 'Death'");
        }

        // ������� ������ ��� �������
        private object GetField(string fieldName)
        {
            var field = typeof(ThirdPersonController).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(controller);
        }

        private void SetField(string fieldName, object value)
        {
            var field = typeof(ThirdPersonController).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(controller, value);
        }

        private void InvokeMethod(string methodName)
        {
            var method = typeof(ThirdPersonController).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(controller, null);
            }
            else
            {
                Assert.Fail($"����� {methodName} �� ��������!");
            }
        }
    }
}