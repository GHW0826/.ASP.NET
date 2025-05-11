## 1.BasicAPI_Route   
- 기본적인 API 설정   
   
## 2.CRUDAPI   
- In-Memory 기반 CRUD API 구현   
- Create, Read, Update, Delete 구성   
- 기본 데이터 조작 API 구성    
   
## 3.DTO_AutoMapper   
- DTO(Data Transfer Object) 사용 및 AutoMapper 적용   
- DTO로 직접 Entity 노출 방지, 매핑 자동화   
   
## 4.EFCore   
- Entity Framework Core로 DB 연동   
- ORM 기반으로 DB 접근   
   
## 5.FluentValidation   
- DTO 입력 값 유효성 검증 (Client → Server)   
- Fluent API로 유효성 규칙 설정   
- 사용자 입력 값의 유효성을 서버에서 자동 검사   
- 클라이언트 요청 데이터의 유효성 검증 및 에러 메시지 통일   
   
## 6.GlobalExceptionMiddleware   
- 전역 예외 처리 (Global Exception Handling Middleware)   
- 모든 예외를 통합 처리, 예외 메시지 사용자 정의 가능   
- 서버에서 발생하는 모든 예외에 대한 일관된 응답 처리   
   
## 7.Swagger   
- API 문서화 (Swagger UI, OpenAPI)   
- API 경로, 요청/응답 구조 자동 생성   
- API 테스트 및 문서화   
   
## 8.APIVersionManage   
- API 버전 관리 (v1, v2)   
- URL 버전 관리 (/api/v1/...), 헤더 버전 관리 가능   
- API 버전 관리로 변경된 기능 안전 적용   
   
## 9.JWTToken   
- JWT (Json Web Token) 기반 인증   
- Access Token (JWT), Refresh Token 구조   
- 사용자 인증, 클라이언트가 서버와 안전하게 통신 가능   
   
## 10.RoleAuthorize   
- 사용자 권한 기반 API 접근 제어   
- [Authorize(Roles = "Admin")]로 특정 Role 사용자만 접근 가능   
- 관리자, 일반 사용자 등 역할 기반 접근 통제   
   
## 11.SwaggerJWTAuthorize   
- Swagger UI에서 JWT 인증 연동   
- Swagger UI에서 Access Token 직접 입력 가능   
- 클라이언트가 JWT 인증이 필요한 API를 쉽게 테스트 가능   
- API 테스트 환경에서 JWT 인증 직접 연동   
   
## 12.DBAuth   
- DB 기반 사용자 인증 (Username, Password)   
- 로그인 API, 사용자 검증, DB 연동   
- 사용자 인증 정보를 DB에서 안전하게 관리   
- 사용자 로그인, 인증 처리   
   
## 13.RefreshTokens   
- Refresh Token을 통한 Access Token 갱신   
- Refresh Token 저장, 만료 시간 관리   
- 사용자 인증 세션 연장 가능, 보안성 강화   
- Access Token 만료 시 자동 갱신 처리   
   
## 14.PasswordBcrypt   
- 사용자 비밀번호 Bcrypt 해싱   
- Bcrypt.Net으로 안전한 비밀번호 저장   
- 사용자 비밀번호 안전성 보장 (해싱 + 솔팅)   
- 안전한 비밀번호 저장 및 인증   
   
## 15.SignupWithdrawal   
- 사용자 회원가입 및 탈퇴 API   
- 회원가입 (POST /register), 회원 탈퇴 (DELETE /users/{id})   
- 사용자 생성 및 삭제 처리   
- 회원가입 및 회원 관리 API   
   
## 16.ResponseFomatSet   
- API 응답 포맷 통일 (ApiResponse<T>) 적용   
- 일관된 응답 구조 (Success, Message, Data)   
- 클라이언트가 응답 파싱 및 에러 처리 용이   
- 모든 API 응답 형식을 통일 (성공/실패 메시지 포함)   
   
## 17.CORSPolicys   
- CORS 정책 설정 (Cross-Origin Resource Sharing)   
- 특정 도메인만 API 호출 허용   
- 보안 강화 (브라우저 기반 공격 방지)   
- 프론트엔드에서 API 호출 허용   
   
## 18.HttpHealthcheck   
- Health Check Endpoint 설정 (/health)   
- API 서버 상태, DB 상태 확인 가능   
- 운영 환경에서 서버 상태 모니터링 가능   
- 로드 밸런서, 모니터링 도구에서 서버 상태 확인   
   
## 19.ThreeTierLayer   
- Controller → Service → Repository 구조로 분리   
- 계층별 역할 분리 (입출력, 비즈니스 로직, DB 처리)   
- 구조적이고 확장 가능한 API 설계   
   
## 20.Dockerfiles   
- API 서버 + DB 도커화 (Dockerfile, docker-compose.yml)   
- 컨테이너 기반 배포, API + DB 통합 실행 가능   
- 어디서든 일관된 환경에서 API 실행 가능   
- 로컬, 클라우드에서 빠르고 일관된 배포   
   
