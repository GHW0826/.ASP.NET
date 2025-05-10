## 1.BasicAPI_Route   
기본적인 API 설정
   
## 2.CRUDAPI   
설명: In-Memory 기반 CRUD API 구현
특징: Create, Read, Update, Delete 구성
예제: Todo CRUD API
사용 목적: 기본 데이터 조작 API 구성 
   
## 3.DTO_AutoMapper   
설명: DTO(Data Transfer Object) 사용 및 AutoMapper 적용
특징: 클라이언트 ↔ 서버 간 데이터 전송 분
사용 목적: DTO로 직접 Entity 노출 방지, 매핑 자동화
   
## 4.EFCore   
설명: Entity Framework Core로 DB 연동
사용 목적: ORM 기반으로 DB 접근, Code-First 개발
   
## 5.FluentValidation   
설명: DTO 입력 값 유효성 검증 (Client → Server)
특징: Fluent API로 유효성 규칙 설정
중요성: 사용자 입력 값의 유효성을 서버에서 자동 검사
사용 목적: 클라이언트 요청 데이터의 유효성 검증 및 에러 메시지 통일

   
## 6.GlobalExceptionMiddleware   
설명: 전역 예외 처리 (Global Exception Handling Middleware)
특징: 모든 예외를 통합 처리, 예외 메시지 사용자 정의 가능
사용 목적: 서버에서 발생하는 모든 예외에 대한 일관된 응답 처리

   
## 7.Swagger   
설명: API 문서화 (Swagger UI, OpenAPI)
특징: API 경로, 요청/응답 구조 자동 생성
사용 목적: API 테스트 및 문서화
   
## 8.APIVersionManage
설명: API 버전 관리 (v1, v2)
특징: URL 버전 관리 (/api/v1/...), 헤더 버전 관리 가능
사용 목적: API 버전 관리로 변경된 기능 안전 적용
   
## 9.JWTToken   
설명: JWT (Json Web Token) 기반 인증
특징: Access Token (JWT), Refresh Token 구조
사용 목적: 사용자 인증, 클라이언트가 서버와 안전하게 통신 가능
   
## 10.RoleAuthorize   
설명: 사용자 권한 기반 API 접근 제어
특징: [Authorize(Roles = "Admin")]로 특정 Role 사용자만 접근 가능
사용 목적: 관리자, 일반 사용자 등 역할 기반 접근 통제
   
## 11.SwaggerJWTAuthorize   
설명: Swagger UI에서 JWT 인증 연동
특징: Swagger UI에서 Access Token 직접 입력 가능
- 클라이언트가 JWT 인증이 필요한 API를 쉽게 테스트 가능
사용 목적: API 테스트 환경에서 JWT 인증 직접 연동
   
## 12.DBAuth   
설명: DB 기반 사용자 인증 (Username, Password)
특징: 로그인 API, 사용자 검증, DB 연동
 사용자 인증 정보를 DB에서 안전하게 관리
사용 목적: 사용자 로그인, 인증 처리
   
## 13.RefreshTokens   
설명: Refresh Token을 통한 Access Token 갱신
특징: Refresh Token 저장, 만료 시간 관리
중요성: 사용자 인증 세션 연장 가능, 보안성 강화
사용 목적: Access Token 만료 시 자동 갱신 처리
   
## 14.PasswordBcrypt   
설명: 사용자 비밀번호 Bcrypt 해싱
특징: Bcrypt.Net으로 안전한 비밀번호 저장
중요성: 사용자 비밀번호 안전성 보장 (해싱 + 솔팅)
사용 목적: 안전한 비밀번호 저장 및 인증
   
## 15.SignupWithdrawal   
설명: 사용자 회원가입 및 탈퇴 API
특징: 회원가입 (POST /register), 회원 탈퇴 (DELETE /users/{id})
중요성: 사용자 생성 및 삭제 처리
사용 목적: 회원가입 및 회원 관리 API
   
## 16.ResponseFomatSet   
설명: API 응답 포맷 통일 (ApiResponse<T>) 적용
특징: 일관된 응답 구조 (Success, Message, Data)
중요성: 클라이언트가 응답 파싱 및 에러 처리 용이
사용 목적: 모든 API 응답 형식을 통일 (성공/실패 메시지 포함)
   
## 17.CORSPolicys   
설명: CORS 정책 설정 (Cross-Origin Resource Sharing)
특징: 특정 도메인만 API 호출 허용
중요성: 보안 강화 (브라우저 기반 공격 방지)
사용 목적: 프론트엔드에서 API 호출 허용
   
## 18.HttpHealthcheck   
설명: Health Check Endpoint 설정 (/health)
특징: API 서버 상태, DB 상태 확인 가능
중요성: 운영 환경에서 서버 상태 모니터링 가능
사용 목적: 로드 밸런서, 모니터링 도구에서 서버 상태 확인
   
## 19.ThreeTierLayer   
설명: Controller → Service → Repository 구조로 분리
특징: 계층별 역할 분리 (입출력, 비즈니스 로직, DB 처리)
중요성: 유지보수성 향상, 테스트 용이
사용 목적: 구조적이고 확장 가능한 API 설계
   
## 20.Dockerfiles   
설명: API 서버 + DB 도커화 (Dockerfile, docker-compose.yml)
특징: 컨테이너 기반 배포, API + DB 통합 실행 가능
중요성: 어디서든 일관된 환경에서 API 실행 가능
사용 목적: 로컬, 클라우드에서 빠르고 일관된 배포
   
