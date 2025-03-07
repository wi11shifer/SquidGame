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
            // Створюємо GameObject для гравця
            playerObject = new GameObject("Player");
            playerObject.SetActive(true);

            // Додаємо компоненти
            controller = playerObject.AddComponent<ThirdPersonController>();
            characterController = playerObject.AddComponent<CharacterController>();
            animator = playerObject.AddComponent<Animator>();
            inputs = playerObject.AddComponent<StarterAssetsInputs>();

            // Налаштовуємо основну камеру
            mainCamera = new GameObject("MainCamera");
            mainCamera.tag = "MainCamera";

            // Встановлюємо початкові значення
            controller.MoveSpeed = 2.0f;
            controller.SprintSpeed = 5.335f;
            controller.TopClamp = 70.0f;
            controller.BottomClamp = -30.0f;

            // Імітуємо CinemachineCameraTarget
            controller.CinemachineCameraTarget = new GameObject("CinemachineTarget");

            // Встановлюємо приватні поля через рефлексію
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

            // Викликаємо Start() для ініціалізації
            InvokeMethod("Start");

            // Перевіряємо налаштування
            Assert.IsNotNull(playerObject, "playerObject не має бути null");
            Assert.IsNotNull(animator, "Компонент Animator має бути прикріплений");
            Assert.IsNotNull(GetField("_animator"), "Поле _animator має бути встановлене");
            Assert.IsTrue((bool)GetField("_hasAnimator"), "_hasAnimator має бути true");
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
            Assert.IsNotNull(GetField("_controller"), "_controller не має бути null перед Move");
            Assert.IsNotNull(GetField("_animator"), "_animator не має бути null перед Move");
            Assert.IsTrue((bool)GetField("_hasAnimator"), "_hasAnimator має бути true");

            SetField("_isDead", false);
            inputs.move = new Vector2(0f, 1f);
            inputs.sprint = true;
            SetField("_speed", 0f);
            SetField("_controller.velocity", Vector3.zero);

            // Act
            InvokeMethod("Move");

            // Assert
            float speed = (float)GetField("_speed");
            Assert.IsTrue(speed > 0f && speed <= controller.SprintSpeed, "Швидкість має відображати SprintSpeed під час спринту");
        }

        [Test]
        public void Move_DoesNotChangeSpeedWhenControllerDisabled()
        {
            // Arrange - негативний сценарій: CharacterController відключений
            Assert.IsNotNull(GetField("_controller"), "_controller не має бути null перед Move");
            characterController.enabled = false; // Вимикаємо CharacterController
            SetField("_isDead", false);
            inputs.move = new Vector2(0f, 1f); // Задаємо рух
            inputs.sprint = true;
            SetField("_speed", 0f);
            SetField("_controller.velocity", Vector3.zero);

            // Act
            InvokeMethod("Move");

            // Assert - швидкість не повинна змінитися
            float speed = (float)GetField("_speed");
            Assert.AreEqual(0f, speed, "Швидкість не повинна змінюватися, коли CharacterController відключений");
        }

        [Test]
        public void AssignAnimationIDs_SetsCorrectHashes()
        {
            // Arrange - очищаємо значення перед тестом
            SetField("_animIDSpeed", 0);
            SetField("_animIDGrounded", 0);
            SetField("_animIDJump", 0);
            SetField("_animIDFreeFall", 0);
            SetField("_animIDMotionSpeed", 0);
            SetField("_animIDDeath", 0);

            // Act - викликаємо метод через рефлексію
            var method = typeof(ThirdPersonController).GetMethod("AssignAnimationIDs", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                Assert.Fail("Метод AssignAnimationIDs не знайдено!");
                return;
            }
            method.Invoke(controller, null);

            // Assert - перевіряємо, чи правильно встановлені хеші
            Assert.AreEqual(Animator.StringToHash("Speed"), GetField("_animIDSpeed"), "animIDSpeed має відповідати хешу 'Speed'");
            Assert.AreEqual(Animator.StringToHash("Grounded"), GetField("_animIDGrounded"), "animIDGrounded має відповідати хешу 'Grounded'");
            Assert.AreEqual(Animator.StringToHash("Jump"), GetField("_animIDJump"), "animIDJump має відповідати хешу 'Jump'");
            Assert.AreEqual(Animator.StringToHash("FreeFall"), GetField("_animIDFreeFall"), "animIDFreeFall має відповідати хешу 'FreeFall'");
            Assert.AreEqual(Animator.StringToHash("MotionSpeed"), GetField("_animIDMotionSpeed"), "animIDMotionSpeed має відповідати хешу 'MotionSpeed'");
            Assert.AreEqual(Animator.StringToHash("Death"), GetField("_animIDDeath"), "animIDDeath має відповідати хешу 'Death'");
        }

        // Допоміжні методи для рефлексії
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
                Assert.Fail($"Метод {methodName} не знайдено!");
            }
        }
    }
}